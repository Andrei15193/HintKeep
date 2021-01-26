using System;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class UserEntity : TableEntity
    {
        public string Email { get; set; }
    }
}