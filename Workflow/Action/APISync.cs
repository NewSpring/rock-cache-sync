using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using RestSharp;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using cc.newspring.CacheBreak.Utilities;

namespace cc.newspring.CacheBreak.Workflow.Action
{
    [Description( "Sync to a third party cache" )]
    [Export( typeof( Rock.Workflow.ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Cache Break Sync" )]
    [CustomDropdownListField( "Action", "The workflow that this action is under is triggered by what type of event", "Save,Delete", true )]
    [TextField( "Sync URL", "The specific URL endpoint this related entity type should synchronize with", true, "" )]
    [TextField( "Token Name", "The key by which the token should be identified in the header of HTTP requests", false, "" )]
    [TextField( "Token Value", "The value of the token to authenticate with the URL endpoint", false, "" )]
    public class CacheBreak : Rock.Workflow.ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var castedEntity = entity as IEntity;
            var isSave = GetAttributeValue( action, "Action" ) == "Save";
            var url = GetAttributeValue( action, "SyncURL" );
            var tokenName = GetAttributeValue( action, "TokenName" );
            var tokenValue = GetAttributeValue( action, "TokenValue" );
            var lastSlash = "/";

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return true;
            }

            if ( url.EndsWith( lastSlash ) )
            {
                lastSlash = string.Empty;
            }

            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( tokenName, tokenValue );

            var fullUrl = string.Format( "{0}{1}{2}", url, lastSlash, castedEntity.Id );
            var client = new RestClient( fullUrl );
            var data = new CacheData();

            data.Entity = castedEntity;
            data.Id = castedEntity.Id;
            data.Action = isSave ? "Save" : "Delete";

            request.JsonSerializer = new NewtonsoftJsonSerializer( request.JsonSerializer );
            request.AddJsonBody( data );

            var response = client.Execute( request );
            return true;
        }
    }
}