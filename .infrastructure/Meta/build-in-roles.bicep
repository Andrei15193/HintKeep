// For a full list check https://learn.microsoft.com/azure/role-based-access-control/built-in-roles
@export()
var buildInRoles = {
  privileged: {
    contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
    owner: '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
    roleBasedAccessControlAdministrator: 'f58310d9-a9f6-439a-9e8d-f62e7b41a168'
    userAccessAdministrator: '18d7d88d-d35e-4fb5-a5c3-7773c20a72d9'
  }

  general: {
    reader: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
  }

  storage: {
    storageAccount: {
      contributor: '17d1049b-9a84-46fb-8f53-869881c3d3ab'

      blob: {
        dataContributor: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
        dataOwner: 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
        dataReader: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
        delegator: 'db58b8e5-c6ad-4a2a-8342-4190687cbf4a'
      }

      queue: {
        dataContributor: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
        dataMessageProcessor: '8a0f0c08-91a1-4084-bc3d-661d67233fed'
        dataMessageSender: 'c6a89b2d-59bc-44d0-9896-0f6e12d7b80a'
        dataReader: '19e7f393-937e-4f77-808e-94535e297925'
      }

      table: {
        dataContributor: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
        dataReader: '76199698-9eea-4c19-bc75-cec21354c6b6'
      }
    }
  }
}
