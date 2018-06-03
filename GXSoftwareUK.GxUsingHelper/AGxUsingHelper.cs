using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GXSoftwareUK.GxUsingHelper
{
    public abstract class AGxUsingHelper
    {
        /// <summary>
        /// Used for mocking as this allows a concrete class to be using instead of instatiating
        /// a new class using reflection
        /// Not forgetting to add [assembly: InternalsVisibleTo(<YourTestClass />)] this this assemble
        /// </summary>
        public virtual object PassedObject { get; set; }

        public virtual T Using<T>(object[] args = null) where T : class
        {
            if (PassedObject != null) return (T)PassedObject;

            if (args != null && args.Any()) return (T)Activator.CreateInstance(typeof(T), args);

            return (T)Activator.CreateInstance(typeof(T), new object[] { });
        }

        public virtual void Close<T>(ref T value)
        {
            PassedObject = null;
            value = default(T);
        }
    }
}
