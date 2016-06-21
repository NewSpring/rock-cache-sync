using cc.newspring.CacheBreak.Utilities;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace cc.newspring.CacheBreak.Workflow.Action
{
    [Description( "Sync to a third party cache" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Cache Break Sync" )]
    [CustomDropdownListField( "Action", "The workflow that this action is under is triggered by what type of event", "Save,Delete", true )]
    [TextField( "Sync URL", "The specific URL endpoint this related entity type should synchronize with", true, "https://" )]
    [TextField( "Token Name", "The key by which the authentication token should be identified in the header of HTTP requests", false, "Authorization-Token")]
    [PersonField( "Rest Key Holder", "Define a REST key within \"Admin Tools/Security/REST Keys\" that will be used for the header token value", false )]
    public class CacheBreak : Rock.Workflow.ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var castedEntity = entity as IEntity;
            var isSave = GetAttributeValue( action, "Action" ) == "Save";
            var url = GetAttributeValue( action, "SyncURL" );
            var tokenName = GetAttributeValue( action, "TokenName" );
            var aliasGuidString = GetAttributeValue( action, "RestKeyHolder");

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return true;
            }

            var request = new RestRequest( Method.POST );
            var client = new RestClient(url);
            var data = new Dictionary<string, object>();
            request.RequestFormat = DataFormat.Json;
            Guid aliasGuid;

            if (!string.IsNullOrWhiteSpace(tokenName) && !string.IsNullOrWhiteSpace(aliasGuidString) && Guid.TryParse(aliasGuidString, out aliasGuid))
            {
                var user = new UserLoginService(rockContext).Queryable().FirstOrDefault(u => u.Person.Aliases.Any(a => a.Guid == aliasGuid));

                if(user != null)
                {
                    request.AddHeader(tokenName, user.ApiKey);
                }          
            }

            data.Add("type", castedEntity.TypeName);
            data.Add("id", castedEntity.Id);
            data.Add("action", isSave ? "save" : "delete");

            request.JsonSerializer = new NewtonsoftJsonSerializer( request.JsonSerializer );
            request.AddJsonBody( data );

            var response = client.Execute( request );
            return true;
        }
    }
}