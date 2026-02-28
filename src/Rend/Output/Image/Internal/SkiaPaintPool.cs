using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Rend.Output.Image.Internal
{
    /// <summary>
    /// A simple object pool for <see cref="SKPaint"/> instances to reduce GC pressure
    /// during rendering operations.
    /// </summary>
    internal sealed class SkiaPaintPool : IDisposable
    {
        private readonly Stack<SKPaint> _pool = new Stack<SKPaint>();
        private readonly List<SKPaint> _allPaints = new List<SKPaint>();
        private bool _disposed;

        /// <summary>
        /// Rents an <see cref="SKPaint"/> from the pool, creating a new one if the pool is empty.
        /// The paint is reset to default state before returning.
        /// </summary>
        /// <returns>An SKPaint ready for use.</returns>
        internal SKPaint Rent()
        {
            if (_pool.Count > 0)
            {
                var paint = _pool.Pop();
                paint.Reset();
                return paint;
            }

            var newPaint = new SKPaint();
            _allPaints.Add(newPaint);
            return newPaint;
        }

        /// <summary>
        /// Returns an <see cref="SKPaint"/> to the pool for reuse.
        /// </summary>
        /// <param name="paint">The paint to return.</param>
        internal void Return(SKPaint paint)
        {
            if (paint != null)
            {
                _pool.Push(paint);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _pool.Clear();
                foreach (var paint in _allPaints)
                {
                    paint.Dispose();
                }
                _allPaints.Clear();
            }
        }
    }
}
