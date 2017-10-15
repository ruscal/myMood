using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Discover
{
    public static class SequentialGuid
    {
        private static int combOffset;
        private static long combTicks = DateTime.UtcNow.Ticks;
        private static object combLock = new object();

        /// <summary>
        /// Creates a new Guid which is partly modified such that they are sequential over time (intended primarily to reduce page fragmentation when used as clustering keys in a database)
        /// </summary>
        /// <remarks>
        /// See "The Cost of GUIDs as Primary Keys" by Jimmy Nilson ( http://www.informit.com/articles/article.aspx?p=25862 ) for more info on the theory behind this.
        /// <para></para>
        /// This implementation trades off less random bytes for more sequential (time-based) bytes in order to give better separation for generation patterns that are "bursty" in nature
        /// (i.e. long periods where value are not generated, punctuated by instances where many values need to be generated in a short space of time)
        /// </remarks>
        /// <returns></returns>
        public static Guid NewCombGuid()
        {
            var ticks = DateTime.UtcNow.Ticks;

            lock (combLock)
            {
                if (ticks > combTicks + combOffset)
                {
                    combTicks = ticks;
                    combOffset = 0;
                }
                else
                {
                    ticks = combTicks + combOffset++;
                }
            }

            byte[] dateBytes = BitConverter.GetBytes(ticks);
            byte[] guidBytes = Guid.NewGuid().ToByteArray();

            // NB: this byte ordering is optimised to fit
            // how SQL Server sorts UUIDs
            guidBytes[8] = dateBytes[1];
            guidBytes[9] = dateBytes[0];
            guidBytes[10] = dateBytes[7];
            guidBytes[11] = dateBytes[6];
            guidBytes[12] = dateBytes[5];
            guidBytes[13] = dateBytes[4];
            guidBytes[14] = dateBytes[3];
            guidBytes[15] = dateBytes[2];

            return new Guid(guidBytes);
        }
    }
}
