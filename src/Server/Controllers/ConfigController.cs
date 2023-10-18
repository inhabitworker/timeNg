using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private IQueryService _query;
        private ICommandService _command;

        public ConfigController(IQueryService query, ICommandService command) 
        { 
            _query = query;
            _command = command;
        }

        /// <summary>
        /// Returns the user config json, or a new instance if there is none on file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        public async Task<ActionResult<ConfigDTO>> GetConfig()
        {
            return await _query.Config();
        }

        /// <summary>
        /// Get theme relevant parts of the config in CSS property format. This is a bit too specific to specific wasm client.
        /// </summary>
        /// <returns></returns>
        [HttpGet("css")]
        public async Task<ActionResult<Dictionary<string, string>>> GetConfigCss()
        {
            var config = await _query.Config();
            return config.ToCss();
        }

        /// <summary>
        /// Returns theming configs as css applied to :root.
        /// </summary>
        /// <returns></returns>
        [HttpGet("css/file")]
        public async Task<FileContentResult> GetConfigCssFile()
        {
            var config = await _query.Config();

            // HttpContext.Response.ContentType = "text/css";
            // await HttpContext.Response.WriteAsync(css);
           
            return File(config.ToCssAppend(), "text/css");
        }

        /// <summary>
        /// Submit an ammended user config json.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult<ConfigDTO>> SetConfig(ConfigDTO config)
        {
            await _command.SetConfig(config);
            return await GetConfig();
        }
    }
}
