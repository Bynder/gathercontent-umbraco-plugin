using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace GatherContent.Connector.UmbracoWebControllers.Controllers
{
    [PluginController("GatherContent")]
    [global::Umbraco.Web.Trees.Tree("GatherContent", "GatherContentTree", "Gather Content", iconClosed: "icon-newspaper-alt")]
    public class GatherContentTreeController : TreeController
    {
        //private readonly IMappingManager _mappingManager;

        //public GatherContentTreeController(IMappingManager mappingManager)
        //{
        //    _mappingManager = mappingManager;
        //}

        // GET: CustomSectionTree
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            var settings = this.CreateTreeNode("settings", id, queryStrings, "Account Settings", "icon-umb-settings", false);
            settings.RoutePath = "GatherContent/GatherContentTree/settings/index";
            var mappings = this.CreateTreeNode("mappings", id, queryStrings, "Mappings", "icon-command", false);
            mappings.RoutePath = "GatherContent/GatherContentTree/mappings/index";
            //mappings.ChildNodesUrl = "backoffice/GatherContent/MappingMenu/GetMappingsNode";

            var import = this.CreateTreeNode("import", id, queryStrings, "Import", "icon-download-alt", false);
            var importWithLocations = this.CreateTreeNode("import.locations", id, queryStrings, "Multilocation Import", "icon-download-alt", false);
            var update = this.CreateTreeNode("update", id, queryStrings, "Update", "icon-conversation", false);
            nodes.Add(settings);

            import.RoutePath = "GatherContent/GatherContentTree/import/index";
            importWithLocations.RoutePath = "GatherContent/GatherContentTree/import.locations/index";

            update.RoutePath = "GatherContent/GatherContentTree/update/index";
            nodes.Add(mappings);
            nodes.Add(import);
            nodes.Add(importWithLocations);
            nodes.Add(update);

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            if (id == "mappings")
            {
                //menu.Items.Add<CreateChildEntity, ActionNew>(ui.Text("actions", ActionNew.Instance.Alias), false, );
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
            }
            if (id == Constants.System.Root.ToInvariantString())
            {
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
            }
            return menu;
        }
    }
}