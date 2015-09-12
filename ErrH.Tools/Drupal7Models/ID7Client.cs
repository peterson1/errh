﻿using System;
using System.Threading.Tasks;
using ErrH.Tools.Authentication;
using ErrH.Tools.Drupal7Models.Entities;
using ErrH.Tools.FileSystemShims;
using ErrH.Tools.ScalarEventArgs;

namespace ErrH.Tools.Drupal7Models
{
    public interface ID7Client : ISessionClient
    {
        D7User CurrentUser          { get; }

        void LoginUsingCredentials(object sender, EArg<LoginCredentials> evtArg);


        Task<T> Get<T>(string resource, string taskTitle = null,
                       string successMsg = null,
                       params Func<T, object>[] successMsgArgs
                       ) where T : new();


        Task<T> Post<T>(T d7Node) where T : D7NodeBase, new();


        Task<T> Put<T>(T nodeRevision, string taskTitle = null, string successMessage = null, params Func<T, object>[] successMsgArgs) where T : ID7NodeRevision, new();

        Task<T> Node<T>(int nodeId) where T : D7NodeBase, new();


        Task<bool> DeleteFile(int fid);


        // Updates specific fields of a node.
        //Task<bool> Put(int nid, Dictionary<string, object> parameters);




        // Attaches file to node.
        //Task<bool>  Post  (FileShim file, int nid);


        /// <summary>
        /// Uploads file to server.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="serverFoldr"></param>
        /// <param name="isPrivate"></param>
        /// <returns>Returns fid on success, otherwise -1.</returns>
        Task<int> Post(FileShim file,
                       string serverFoldr = "",
                       bool isPrivate = true);
    }
}
