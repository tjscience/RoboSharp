using System;

namespace RoboSharp
{
    /// <summary>
    /// A pre-.NET4 Lazy<T> implementation
    /// http://dannykendrick.blogspot.com/2012/10/lazy-net-35.html
    /// </summary>
    public class Lazy<T>
     where T : class
    {
        private readonly object padlock;
        private readonly Func<T> function;

        private bool hasRun;
        private T instance;

        public Lazy(Func<T> function)
        {
            this.hasRun = false;
            this.padlock = new object();
            this.function = function;
        }

        public T Value()
        {
            lock (padlock)
            {
                if (!hasRun)
                {
                    instance = function.Invoke();

                    hasRun = true;
                }
            }

            return instance;
        }
    }
}
