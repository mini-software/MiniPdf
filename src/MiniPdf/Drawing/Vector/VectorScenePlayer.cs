namespace MiniPdf.Drawing.Vector
{
    /// <summary>
    /// Replays a <see cref="VectorScene"/> onto any <see cref="IDrawingContext"/>.
    /// One replay engine, three outputs: raster (Bitmap), SVG, EMF.
    /// </summary>
    internal static class VectorScenePlayer
    {
        /// <summary>
        /// Replays each command in the scene in order onto the specified target.
        /// State commands mutate target state; primitives draw.
        /// Save/Restore use the target's own state stack.
        /// </summary>
        /// <param name="scene">The scene to replay.</param>
        /// <param name="target">The target drawing context.</param>
        public static void Play(VectorScene scene, IDrawingContext target)
        {
            if (scene == null) return;
            if (target == null) return;

            foreach (var cmd in scene.Commands)
                cmd.Replay(target);
        }
    }
}