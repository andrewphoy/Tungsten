using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tungsten.Data;

namespace Tungsten.Controllers {
    public class QuestionAnswerController : Controller {

        private readonly ApplicationDbContext _dbContext;
        public QuestionAnswerController(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        [Route("q-and-a")]
        public async Task<IActionResult> QandA() {
            var questions = await _dbContext.Questions.OrderBy(q => q.Order).ToListAsync();
            return View(questions);
        }
    }
}
