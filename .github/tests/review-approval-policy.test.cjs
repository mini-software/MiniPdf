const assert = require('node:assert/strict');
const { existsSync, readFileSync } = require('node:fs');
const { resolve } = require('node:path');
const { test } = require('node:test');

const workflow = readFileSync(resolve(__dirname, '..', 'workflows', 'review-approval-policy.yml'), 'utf8');
const block = workflow.match(/          script: \|\r?\n((?:            [^\r\n]*(?:\r?\n|$)|\r?\n)+)/);
assert.ok(block, 'Expected an embedded approval policy script.');
const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;
const evaluate = new AsyncFunction('github', 'context', 'core', block[1].replace(/^            /gm, ''));

function pull(number = 1, labels = [], sha = `sha-${number}`) {
  return {
    number, draft: false, head: { sha },
    user: { login: 'author' }, labels: labels.map(name => ({ name }))
  };
}

function review(id, login, state = 'APPROVED', type = 'User') {
  return { id, state, user: { login, type } };
}

function harness(pulls, reviews = {}, options = {}) {
  const runs = [...(options.existingChecks ?? [])];
  const writes = [];
  const creates = [];
  const logs = [];
  const errors = [];
  const failed = [];
  const permissionCalls = [];
  const list = async () => {};
  const listReviews = async () => {};
  const listForRef = async () => {};
  let nextId = Math.max(0, ...runs.map(run => run.id)) + 1;
  const github = {
    rest: {
      pulls: { list, listReviews },
      checks: {
        listForRef,
        create: async params => {
          if (options.createError) {
            throw options.createError;
          }
          const run = { ...params, id: nextId++, app: { slug: 'github-actions' } };
          runs.push(run);
          creates.push(params);
          writes.push({ ...run });
          return { data: run };
        },
        update: async params => {
          if (options.startError && params.status === 'in_progress') {
            throw options.startError;
          }
          if (options.failureUpdateError && params.conclusion === 'failure') {
            throw options.failureUpdateError;
          }
          const run = runs.find(run => run.id === params.check_run_id);
          assert.ok(run, 'Only an existing check run may be updated.');
          Object.assign(run, params);
          writes.push({ ...run });
          return { data: run };
        }
      },
      repos: {
        getCollaboratorPermissionLevel: async ({ username }) => {
          permissionCalls.push(username);
          if (options.permissionError) {
            throw options.permissionError;
          }
          return { data: { permission: options.permissions?.[username] ?? 'write' } };
        }
      }
    },
    paginate: async (method, params) => {
      assert.equal(params.per_page, 100);
      if (method === list) {
        assert.equal(params.base, 'main');
        assert.equal(params.state, 'open');
        return pulls;
      }
      if (method === listForRef) {
        assert.equal(params.check_name, 'Review approval policy');
        assert.equal(params.filter, 'all');
        return runs.filter(run => run.head_sha === params.ref);
      }
      assert.equal(method, listReviews);
      const error = options.reviewErrors?.[params.pull_number] ?? options.reviewError;
      if (error) {
        throw error;
      }
      return [...(reviews[params.pull_number] ?? [])];
    }
  };
  const context = {
    repo: { owner: 'mini-software', repo: 'MiniPdf' },
    serverUrl: 'https://github.com', runId: 123,
    payload: {}
  };
  const run = () => evaluate(github, context, {
    info: message => logs.push(message),
    error: message => errors.push(message),
    setFailed: message => failed.push(message)
  });
  return { run, writes, creates, runs, logs, errors, failed, permissionCalls };
}

test('ordinary recommendation is neutral at zero approvals and succeeds at one', async () => {
  for (const [reviews, expected] of [[[], 'neutral'], [[review(1, 'reviewer')], 'success']]) {
    const h = harness([pull()], { 1: reviews });
    await h.run();
    assert.equal(h.writes[0].status, 'in_progress');
    assert.equal(h.writes.at(-1).status, 'completed');
    assert.equal(h.writes.at(-1).conclusion, expected);
    assert.equal(h.writes.at(-1).name, 'Review approval policy');
    assert.match(h.writes.at(-1).output.summary, /Advisory only/);
    assert.match(h.writes.at(-1).output.summary, /maintainers may merge without waiting/);
    assert.deepEqual(h.failed, []);
  }
});

test('major-change recommendation stays neutral below three approvals', async () => {
  for (const count of [0, 1, 2, 3]) {
    const h = harness([pull(1, ['MAJOR-CHANGE'])], {
      1: Array.from({ length: count }, (_, index) => review(index + 1, `reviewer-${index}`))
    });
    await h.run();
    assert.equal(h.writes.at(-1).conclusion, count === 3 ? 'success' : 'neutral');
    assert.deepEqual(h.failed, []);
    if (count < 3) {
      assert.match(h.writes.at(-1).output.summary, new RegExp(`${count}/3`));
      assert.match(h.writes.at(-1).output.title, /Review reminder/);
    }
  }
});

test('invited users, comments, author approvals, bots and Read users do not count', async () => {
  const h = harness([pull(1, ['major-change'])], {
    1: [
      review(1, 'AUTHOR'), review(2, 'bot', 'APPROVED', 'Bot'),
      review(3, 'read-user'), review(4, 'commenter', 'COMMENTED'),
      review(5, 'valid')
    ]
  }, { permissions: { 'read-user': 'read' } });
  await h.run();
  assert.match(h.writes.at(-1).output.summary, /1\/3/);
  assert.deepEqual(h.permissionCalls, ['read-user', 'valid']);
});

test('counts Write, Maintain and Admin approvals', async () => {
  const h = harness([pull(1, ['major-change'])], {
    1: [review(1, 'writer'), review(2, 'maintainer'), review(3, 'admin')]
  }, { permissions: { writer: 'write', maintainer: 'maintain', admin: 'admin' } });
  await h.run();
  assert.equal(h.writes.at(-1).conclusion, 'success');
});

test('later dismissed or changes-requested reviews supersede earlier approvals', async () => {
  for (const state of ['DISMISSED', 'CHANGES_REQUESTED']) {
    const h = harness([pull()], {
      1: [review(2, 'REVIEWER', state), review(1, 'reviewer')]
    });
    await h.run();
    assert.match(h.writes.at(-1).output.summary, /0\/1/);
  }
});

test('comments do not cancel approvals and duplicate approvals count only once', async () => {
  const h = harness([pull(1, ['major-change'])], {
    1: [review(1, 'reviewer'), review(2, 'REVIEWER'), review(3, 'Reviewer', 'COMMENTED')]
  });
  await h.run();
  assert.match(h.writes.at(-1).output.summary, /1\/3/);
});

test('a new approval after a change request is counted', async () => {
  const h = harness([pull()], {
    1: [review(1, 'reviewer', 'CHANGES_REQUESTED'), review(2, 'reviewer')]
  });
  await h.run();
  assert.equal(h.writes.at(-1).conclusion, 'success');
});

test('a label change raises the recommendation to a neutral reminder', async () => {
  const current = pull();
  const h = harness([current], { 1: [review(1, 'reviewer')] });
  await h.run();
  assert.equal(h.writes.at(-1).conclusion, 'success');
  current.labels.push({ name: 'major-change' });
  await h.run();
  assert.equal(h.writes.at(-2).status, 'in_progress');
  assert.equal(h.writes.at(-1).conclusion, 'neutral');
  assert.match(h.writes.at(-1).output.summary, /1\/3/);
  assert.equal(h.creates.length, 1);
});

test('PRs sharing a SHA preserve the higher recommendation without blocking', async () => {
  const h = harness([pull(1, [], 'shared'), pull(2, ['major-change'], 'shared')], {
    1: [review(1, 'one')],
    2: [review(2, 'one'), review(3, 'two')]
  });
  await h.run();
  assert.equal(h.writes.length, 2);
  assert.equal(h.writes.at(-1).head_sha, 'shared');
  assert.equal(h.writes.at(-1).conclusion, 'neutral');
  assert.match(h.writes.at(-1).output.summary, /#2: 2\/3/);
  assert.deepEqual(h.permissionCalls, ['one', 'two']);
});

test('all PRs sharing a SHA meeting recommendations produce success', async () => {
  const h = harness([pull(1, [], 'shared'), pull(2, ['major-change'], 'shared')], {
    1: [review(1, 'one')],
    2: [review(2, 'one'), review(3, 'two'), review(4, 'three')]
  });
  await h.run();
  assert.equal(h.writes.at(-1).conclusion, 'success');
});

test('review and permission API failures fail the check and workflow explicitly', async () => {
  for (const option of ['reviewError', 'permissionError']) {
    const error = new Error('GitHub API access failed');
    const h = harness([pull()], { 1: [review(1, 'reviewer')] }, { [option]: error });
    await h.run();
    assert.deepEqual(h.writes.map(check => check.status), ['in_progress', 'completed']);
    assert.equal(h.writes.at(-1).conclusion, 'failure');
    assert.match(h.writes.at(-1).output.summary, /GitHub API access failed/);
    assert.equal(h.errors.length, 1);
    assert.equal(h.failed.length, 1);
  }
});

test('draft PRs are skipped and no open PRs are reported explicitly', async () => {
  const draft = pull();
  draft.draft = true;
  for (const pulls of [[], [draft]]) {
    const h = harness(pulls);
    await h.run();
    assert.deepEqual(h.writes, []);
    assert.match(h.logs[0], /No open/);
  }
});

test('check summaries obey the 65535-character limit', async () => {
  const h = harness(Array.from({ length: 3000 }, (_, index) => pull(index + 1, ['major-change'], 'shared')));
  await h.run();
  assert.equal(h.writes.at(-1).output.summary.length, 65535);
});

test('refreshing more than 1000 times updates one check instead of appending statuses', async () => {
  const h = harness([pull()], { 1: [review(1, 'reviewer')] });
  for (let iteration = 0; iteration < 1001; iteration++) {
    await h.run();
  }
  assert.equal(h.creates.length, 1);
  assert.equal(h.runs.length, 1);
  assert.equal(h.writes.length, 2002);
  assert.equal(h.writes.at(-1).conclusion, 'success');
});

test('a failed SHA does not prevent other SHA groups from refreshing', async () => {
  const h = harness([pull(1), pull(2)], { 2: [review(1, 'valid')] }, {
    reviewErrors: { 1: new Error('First SHA reviews unavailable') }
  });
  await h.run();
  assert.equal(h.runs.find(run => run.head_sha === 'sha-1').conclusion, 'failure');
  assert.equal(h.runs.find(run => run.head_sha === 'sha-2').conclusion, 'success');
  assert.match(h.failed[0], /1 SHA group/);
});

test('failure to reset an existing success attempts an explicit failure update', async () => {
  const existing = {
    id: 42, head_sha: 'sha-1', external_id: 'minipdf-review-approval-policy',
    app: { slug: 'github-actions' }, status: 'completed', conclusion: 'success'
  };
  const h = harness([pull()], {}, {
    existingChecks: [existing], startError: new Error('Cannot start refresh')
  });
  await h.run();
  assert.equal(h.creates.length, 0);
  assert.equal(h.runs[0].conclusion, 'failure');
  assert.equal(h.failed.length, 1);
});

test('failure to publish a failure is logged and still fails the workflow', async () => {
  const h = harness([pull()], {}, {
    reviewError: new Error('Review API unavailable'),
    failureUpdateError: new Error('Check API unavailable')
  });
  await h.run();
  assert.equal(h.errors.length, 2);
  assert.match(h.errors[1], /Failed to publish failure/);
  assert.equal(h.runs[0].status, 'in_progress');
  assert.equal(h.failed.length, 1);
});

test('check creation failures are explicit and do not prevent other SHA groups', async () => {
  const h = harness([pull(1), pull(2)], {}, { createError: new Error('Check API unavailable') });
  await h.run();
  assert.equal(h.errors.length, 2);
  assert.match(h.failed[0], /2 SHA group/);
  assert.deepEqual(h.writes, []);
});

test('checks belonging to another app or external ID are not updated', async () => {
  const existingChecks = [
    { id: 1, head_sha: 'sha-1', external_id: 'minipdf-review-approval-policy', app: { slug: 'other-app' } },
    { id: 2, head_sha: 'sha-1', external_id: 'other-check', app: { slug: 'github-actions' } }
  ];
  const h = harness([pull()], {}, { existingChecks });
  await h.run();
  assert.equal(h.creates.length, 1);
  assert.ok(h.writes.every(run => run.id === 3));
});

test('deleted review users never count as eligible approvals', async () => {
  const h = harness([pull()], { 1: [{ id: 1, state: 'APPROVED', user: null }, review(2, 'valid')] });
  await h.run();
  assert.equal(h.writes.at(-1).conclusion, 'success');
  assert.deepEqual(h.permissionCalls, ['valid']);
});

test('advisory workflow has only base-controlled triggers and no PR notifier', () => {
  assert.match(workflow, /pull_request_target:/);
  assert.match(workflow, /branches: \[main\]/);
  assert.match(workflow, /schedule:/);
  assert.match(workflow, /cron: '7,22,37,52 \* \* \* \*'/);
  assert.match(workflow, /workflow_dispatch:/);
  assert.match(workflow, /checks: write/);
  assert.match(workflow, /pull-requests: read/);
  assert.doesNotMatch(workflow, /actions\/checkout|\$\{\{[^}]*head\.(?:sha|ref)|download-artifact|secrets\.|pull_request_review:|workflow_run:|statuses: write|createCommitStatus/);
  assert.equal(existsSync(resolve(__dirname, '..', 'workflows', 'review-changed.yml')), false);
  const contributing = readFileSync(resolve(__dirname, '..', '..', 'CONTRIBUTING.md'), 'utf8');
  assert.match(contributing, /approval counts are \*\*reminders only\*\*, not merge requirements/);
  assert.match(contributing, /approval minimum for `main` is \*\*zero\*\*/);
  assert.match(contributing, /native `minimum_approvals` set to \*\*zero\*\*/);
  assert.match(contributing, /Without `major-change`: \*\*one approval recommended\*\*/);
  assert.match(contributing, /With `major-change`: \*\*three approvals recommended\*\*/);
  for (const pattern of [
    '.github/**', 'AGENTS.md', 'CONTRIBUTING.md', 'GOVERNANCE.md',
    'SECURITY.md', 'LICENSE', 'MAINTAINERS.md', 'CODE_OF_CONDUCT.md'
  ]) {
    assert.ok(contributing.split(/\r?\n/).includes(pattern), `Document important path: ${pattern}`);
  }
  assert.match(contributing, /The label changes only the recommendation, not merge eligibility/);
  assert.match(contributing, /empty\s+bypass list/);
  assert.match(workflow, /No approving reviews are required to merge/);
  assert.match(workflow, /conclusion: waiting.length > 0 \? 'neutral' : 'success'/);
  assert.doesNotMatch(workflow, /action_required|native three-approval protection|Native rules require one approval/);
});
