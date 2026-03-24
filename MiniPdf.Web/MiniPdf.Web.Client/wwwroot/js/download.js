window.downloadFile = function (fileName, contentType, byteArray) {
    var blob = new Blob([byteArray], { type: contentType });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// --- IndexedDB font cache ---
(function () {
    var DB_NAME = 'minipdf-fonts';
    var STORE_NAME = 'fonts';
    var DB_VERSION = 1;

    function openDb() {
        return new Promise(function (resolve, reject) {
            var req = indexedDB.open(DB_NAME, DB_VERSION);
            req.onupgradeneeded = function (e) {
                var db = e.target.result;
                if (!db.objectStoreNames.contains(STORE_NAME)) {
                    db.createObjectStore(STORE_NAME);
                }
            };
            req.onsuccess = function (e) { resolve(e.target.result); };
            req.onerror = function (e) { reject(e.target.error); };
        });
    }

    window.fontCache_get = async function (key) {
        try {
            var db = await openDb();
            return await new Promise(function (resolve, reject) {
                var tx = db.transaction(STORE_NAME, 'readonly');
                var store = tx.objectStore(STORE_NAME);
                var req = store.get(key);
                req.onsuccess = function () { resolve(req.result || null); };
                req.onerror = function () { resolve(null); };
            });
        } catch (e) {
            return null;
        }
    };

    window.fontCache_set = async function (key, data) {
        try {
            var db = await openDb();
            return await new Promise(function (resolve, reject) {
                var tx = db.transaction(STORE_NAME, 'readwrite');
                var store = tx.objectStore(STORE_NAME);
                store.put(data, key);
                tx.oncomplete = function () { resolve(true); };
                tx.onerror = function () { resolve(false); };
            });
        } catch (e) {
            return false;
        }
    };
})();
