﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrH.Drupal7RepoUpdater.DTOs;
using ErrH.Tools.CollectionShims;
using ErrH.Tools.Drupal7Models;
using ErrH.Tools.Drupal7Models.Entities;
using ErrH.Tools.Drupal7Models.FieldAttributes;
using ErrH.Tools.Extensions;
using ErrH.Tools.Loggers;
using ErrH.Tools.Serialization;
using ErrH.Tools.SqlHelpers;

namespace ErrH.Drupal7RepoUpdater
{
    public abstract class D7RepoSqlUpdaterBase<T>
        : LogSourceBase, IRepoUpdater<T>
        where T : D7NodeBase, new()
    {
        private ISerializer _serialr;



        public D7RepoSqlUpdaterBase(ISerializer serializer)
        {
            _serialr = serializer;
        }


        public ISqlDbReader DbReader { get; set; }


        public abstract string  ResourceURL { get; }
        public virtual  string  SqlQuery    => SqlBuilder.SELECT<T>();


        public async Task<bool> Update(IRepository<T> repo, 
                                       IMapOverride overridr,
                                       CancellationToken token = new CancellationToken())
        {
            var sqlTask  = DbReader.Query(SqlQuery, token);
            var repoTask = QueryTargetD7(repo, ResourceURL, token);

            try { await TaskEx.WhenAll(sqlTask, repoTask); }
            catch (Exception ex)
            { return Error_n("Error on Update()", ex.Details(false, false)); }

            var sqlResult = await sqlTask;
            var repoResult = await repoTask;

            if (sqlResult == null)
                return Error_n("SQL query task returned NULL.", "");

            if (repoResult == null)
                return Error_n("D7 query task returned NULL.", ResourceURL);

            var sC = sqlResult.Count;
            var rC = repo.All.Count;

            Info_n("Total records returned:",
                 $"{sC} in SQL DB; {rC} in D7 server (diff: {sC - rC})");

            if (!ApplyChangesToRepo(sqlResult, 
                repoResult, repo, overridr)) return false;

            Info_n("Saving changes to repo...",
                    $"new nodes: {repo.NewUnsavedItems.Count} ;"
                  + $" modified: {repo.ChangedUnsavedItems.Count}");

            return await repo.SaveChangesAsync(token);
        }


        private async Task<IEnumerable<NodeRecordHash>> QueryTargetD7
            (IRepository<T> repo, string resourceURL, CancellationToken token)
        {
            var client = repo.Client as ID7Client;
            return await client.Get<List<NodeRecordHash>>(resourceURL, token);
        }
        


        private bool ApplyChangesToRepo(RecordSetShim sqlResult, 
                                        IEnumerable<NodeRecordHash> nodeRecHashes, 
                                        IRepository<T> repo,
                                        IMapOverride overrider)
        {
            Info_n("Applying changes to repo...", "");

            var tblKey  = DbColAttribute.Key<T>()?.Property?.Name;
            if (tblKey.IsBlank())
                return Error_n($"DbTable attribute missing from ‹{typeof(T).Name}›", "");

            var hashField = D7HashFieldAttribute.FindIn<T>();

            foreach (var row in sqlResult)
            {
                var dbRecID = row.AsInt(tblKey);
                var dbRowSha1 = _serialr.SHA1(row);

                var repoNode = new T();
                var d7RecHash = nodeRecHashes.FirstOrDefault(x => x.dbID == dbRecID);

                if (d7RecHash == null)
                    repo.Add(repoNode);
                else
                    repoNode = repo.ByNid(d7RecHash.nid);

                if (dbRowSha1 != d7RecHash?.sha1)
                {
                    DbRowMapper.Map(row, repoNode, overrider);

                    hashField?.ModelProperty?
                        .SetValue(repoNode, dbRowSha1, null);
                }

            }
            return true;
        }






        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                DbReader?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _isDisposed = true;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~D7CachedRepoSqlUpdaterBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
