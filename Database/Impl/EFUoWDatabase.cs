using System;

namespace PerformanceTest.Database.Impl
{
    public class EFUoWDatabase : EFDatabase
    {
        private DataContext Context;

        public EFUoWDatabase()
        {
            Context = new DataContext();
        }

        public override void BeginTransaction()
        {
        }

        public override void Commit()
        {
            Context.SaveChanges();
        }

        protected override TReturn Execute<TReturn>(Func<DataContext, TReturn> action)
        {
            return action.Invoke(Context);
        }

        protected override void Execute(Action<DataContext> action)
        {
            action.Invoke(Context);
        }
    }
}