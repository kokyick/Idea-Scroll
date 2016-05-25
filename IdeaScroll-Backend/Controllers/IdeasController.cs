using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using IdeaScroll_Backend.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace IdeaScroll_Backend.Controllers
{
    [Authorize]
    public class IdeasController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: GetIdeaList
        [Route("GetIdeaList")]
        public async Task<IHttpActionResult> GetIdeaList()
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            var query = await db.Idea.Select(i =>
            new
            {
                Description = i.Description,
                Title = i.Title,
                Id = i.Id,
                UserId=i.UserId,
                visibility=i.visible
            }).Where(x => x.UserId == user.Id && x.visibility==true).OrderByDescending(y=>y.Id).ToListAsync();
            return Ok(query);
        }

        // GET: GetIdea
        [Route("GetIdea")]
        public async Task<IHttpActionResult> GetIdea(int id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var query = await db.Idea.Select(i =>
            new
            {
                Description = i.Description,
                Title = i.Title,
                Id = i.Id,
                UserId = i.UserId,
                visibility = i.visible
            }).Where(x => x.UserId == user.Id && x.visibility == true && id==x.Id).ToListAsync();
            return Ok(query);
        }

        // PUT: api/Ideas/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutIdea(int id, AddNewIdea idea)
        {

            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            try
            {
                //check end date passed
                var getIdea = await db.Idea.Where(x => x.Id == id).SingleAsync();

                getIdea.Title = idea.Title;
                getIdea.Description = idea.Description;
                getIdea.LastEdited = DateTimeOffset.UtcNow;

                await db.SaveChangesAsync();
                return Ok("Edited");
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }
        // PUT: api/deleteIdea
        [Route("api/DeleteIdea")]
        public async Task<IHttpActionResult> Put_DeleteIdea(int IdeaId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            try
            {
                //check end date passed
                var getIdea = await db.Idea.Where(x => x.Id == IdeaId).SingleAsync();

                getIdea.visible = false;

                await db.SaveChangesAsync();
                return Ok("delete");
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }


        }

        // POST: api/PostNewIdea
        
        [Route("api/PostNewIdea")]
        public async Task<IHttpActionResult> PostNewIdea(AddNewIdea NewIdea)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var addIdea = new Idea() { LastEdited=DateTimeOffset.UtcNow, Created=DateTimeOffset.UtcNow, visible=true , Description = NewIdea.Description, UserId = user.Id, Title = NewIdea.Title };

            var haha=db.Idea.Add(addIdea);
            await db.SaveChangesAsync();

            return Ok(haha.Id.ToString());
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IdeaExists(int id)
        {
            return db.Idea.Count(e => e.Id == id) > 0;
        }
    }
}