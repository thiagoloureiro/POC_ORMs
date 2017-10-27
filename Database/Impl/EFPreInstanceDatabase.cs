using System;

namespace PerformanceTest.Database.Impl
{
    public class EFPreInstanceDatabase : EFDatabase
    {
        private DataContext transactionContext = null;

        public override void BeginTransaction()
        {
            transactionContext = new DataContext();
        }

        public override void Commit()
        {
            Execute(db => db.SaveChanges());
            transactionContext = null;
        }

        protected override TReturn Execute<TReturn>(Func<DataContext, TReturn> action)
        {
            if(transactionContext  != null)
                return action.Invoke(transactionContext);

            using (var db = new DataContext())
            {
                return action.Invoke(db);
            }
        }

        protected override void Execute(Action<DataContext> action)
        {
            if (transactionContext != null)
            {
                action.Invoke(transactionContext);
                return;
            }

            using (var db = new DataContext())
            {
                action.Invoke(db);
            }
        }
    }
}