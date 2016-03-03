using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace SC.DAL.Dapper
{
    public static class Extensions
    {
        public static IEnumerable<TParent> QueryOneToMany<TParent, TChild>(this IDbConnection conn, string sql, Func<TParent, TChild, TParent> func, object data = null, string splitOn = "id")
        {
            var lookup = new Dictionary<int, TParent>();

            conn.Query<TParent, TChild, TParent>(sql, (p, c) =>
            {
                TParent parent;

                if (!lookup.TryGetValue(p.GetHashCode(), out parent))
                {
                    lookup.Add(p.GetHashCode(), parent = p);
                }

                return func(p, c);

            }, data, splitOn: splitOn);

            return lookup.Values.ToArray();
        }
    }
}
