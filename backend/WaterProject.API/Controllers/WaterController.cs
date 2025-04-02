using Microsoft.AspNetCore.Mvc;
using WaterProject.API.Data;

namespace WaterProject.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WaterController: ControllerBase
    {

        private WaterDbContext _waterContext;
        
        public WaterController(WaterDbContext temp)
        {
            _waterContext = temp;
        }

        [HttpGet("AllProjects")]
        public IActionResult GetProjects(int pageSize = 10, int pageNum = 1,
            [FromQuery]List<string>? projectTypes= null)
        {
            string? favProjType = Request.Cookies["favProjType"];
            Console.WriteLine("COOKIE \n" + favProjType);
            
            HttpContext.Response.Cookies.Append("FavoriteProjectType", "Borehold Well and Hand Pump", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(1),
            });

            var query = _waterContext.Projects.AsQueryable();

            if (projectTypes != null && projectTypes.Any())
            {
                query = query.Where(p => projectTypes.Contains(p.ProjectType));
            }
            
            
            var totalNumProjects = query.Count(); 
            
            var something = query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            

            return Ok(new
            {
                Projects = something,
                TotalNumProjects = totalNumProjects
            });

        }

        [HttpGet("GetProjectTypes")]
        public IActionResult GetProjectTypes()
        {
            var projectTypes = _waterContext.Projects
                .Select(p => p.ProjectType)
                .Distinct()
                .ToList();
            return Ok(projectTypes);
        }


        [HttpPost("AddProject")]
        public IActionResult AddProject([FromBody] Project newProject)
        {
            _waterContext.Projects.Add(newProject);
            _waterContext.SaveChanges();
            return Ok(newProject);

        }

        [HttpPut("UpdateProject/{projectId}")]
        public IActionResult UpdateProject(int projectId, [FromBody] Project updatedProject)
        {
            var existingProject = _waterContext.Projects.Find(projectId);
            existingProject.ProjectName = updatedProject.ProjectName; 
            existingProject.ProjectType = updatedProject.ProjectType;
            existingProject.ProjectRegionalProgram = updatedProject.ProjectRegionalProgram;
            existingProject.ProjectImpact = updatedProject.ProjectImpact;
            existingProject.ProjectPhase = updatedProject.ProjectPhase;
            existingProject.ProjectFunctionalityStatus = updatedProject.ProjectFunctionalityStatus;
            _waterContext.Projects.Update(existingProject);
            _waterContext.SaveChanges();
            
            return Ok(updatedProject);

        }

        [HttpDelete("DeleteProject/{projectId}")]
        public IActionResult DeleteProject(int projectId)
        {
            var project = _waterContext.Projects.Find(projectId);

            if (project == null)
            {
                return NotFound(new {message = "project not found"});
            }
            
            _waterContext.Projects.Remove(project);
            _waterContext.SaveChanges();
            return NoContent();
        }

      
    } 
}
