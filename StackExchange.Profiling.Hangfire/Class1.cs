using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Profiling.Hangfire
{
    public class HangfireProfilerProvider : BaseProfilerProvider
    {
        private MiniProfiler _current;

        public override MiniProfiler GetCurrentProfiler()
        {
            return _current;
        }

        public override MiniProfiler Start(string sessionName = null)
        {
            _current = new MiniProfiler(sessionName);
            //_current.Star
            return _current;
        }

        public override MiniProfiler Start(ProfileLevel level, string sessionName = null)
        {
            throw new NotImplementedException();
        }

        public override void Stop(bool discardResults)
        {
            throw new NotImplementedException();
        }
    }
}
