using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace ValueApp.Services
{
    public class Ressource
    {
        public int Id { get; set; }
    }

    public class Link
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }

    public class RestRessource : Ressource
    {
        [NotMapped]
        public IList<Link> Links { get; set; } = new List<Link>();

        public void AddSelfLink(IUrlHelper uriHelper, string method, string controller)
        {
            Links.Add(new Link() { Href = uriHelper.Action(method, controller, new { id = Id }), Method = method, Rel = "self" });
        }

        public void AddLink(IUrlHelper uriHelper, string method, string controller, string rel, object routeParams)
        {
            Links.Add(new Link() { Href = uriHelper.Action(method, controller, routeParams), Method = method, Rel = rel });
        }
    }


    /*
    public class RestRessource2 : Ressource
    {
        [NotMapped]
        public IDictionary<string, object> Links { get; set; } = new ExpandoObject();

        public void AddSelfLink(IUrlHelper uriHelper, string method, string controller)
        {
            Links["self"] = new Link() { Href = uriHelper.Action(method, controller, new { id = Id }), Method = method, Rel = "self" };
        }

        public void AddLink(IUrlHelper uriHelper, string method, string controller, string rel, object routeParams)
        {
            Links[rel] = new Link() { Href = uriHelper.Action(method, controller, routeParams), Method = method, Rel = rel };
        }
    }
    */
}