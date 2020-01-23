using Blog.Core.IServices;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Tasks.QuartzNet
{
    public class Job1Quartz : JobBase, IJob
    {
        private readonly IBlogArticleServices _blogArticleServices;

        public Job1Quartz(IBlogArticleServices blogArticleServices)
        {
            _blogArticleServices = blogArticleServices;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteJob(context, async () => await Run(context));
        }
        public async Task Run(IJobExecutionContext context)
        {
            try
            {
                var param = context.MergedJobDataMap;
                var list = await _blogArticleServices.Query();
                await Console.Out.WriteLineAsync("菜单表里总数量" + list.Count.ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }

}
