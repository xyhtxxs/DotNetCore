# Z  主要知识点



## AOP 

本项目多处采用面向切面编程思想——AOP，除了广义上的过滤器和中间件以外，主要通过动态代理的形式来实现AOP编程思想，主要的案例共有四个，分别是：  
1、服务日志AOP；  
2、服务InMemory缓存AOP；  
3、服务Redis缓存AOP；  
4、服务事务AOP；  
  
   
具体的代码可以在 `Blog.Core\Blog.Core\AOP` 文件夹下查看。  
  
与此同时，多个AOP也设置了阀门来控制是否开启，具体的可以查看 `appsettings.json` 中的：  

```
  "AppSettings": {
    "RedisCachingAOP": {
      "Enabled": false,
      "ConnectionString": "127.0.0.1:6319"
    },
    "MemoryCachingAOP": {
      "Enabled": true
    },
    "LogAOP": {
      "Enabled": false
    },
    "TranAOP": {
      "Enabled": false
    },
    "SqlAOP": {
      "Enabled": false
    }
  },

```

## Appsettings 

整个系统通过一个封装的操作类 `Appsettings.cs` 来控制配置文件 `appsettings.json` 文件，  
操作类地址在：`\Blog.Core.Common\Helper` 文件夹下。  
具体的使用方法是：  

```
Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "Enabled" })

// 里边的参数，按照 appsettings.json 中设置的层级顺序来写，可以获取到指定的任意内容。

```


## Async-Await 

整个系统采用 async/await 异步编程，符合主流的开发模式，   
特别是对多线程开发很友好。



## Authorization-Ids4

本系统 v1.0 版本（目前的 is4 分支，如果没有该分支，表示已经迁移到主分支）已经实现了对 `IdentityServer4` 的迁移，已经支持了统一授权认证，和 `blog` 项目、`Admin` 项目、`DDD` 项目等一起，使用一个统一的认证中心。  
  
具体的代码参考：`Blog.Core\Blog.Core\Extensions` 文件夹下的 `AuthorizationSetup.cs` 中 `Ids4` 认证的部分，注意需要引用指定的 `nuget` 包：   

```
  // 2.添加Identityserver4认证
  .AddIdentityServerAuthentication(options =>
  {
      options.Authority = "https://ids.neters.club";
      options.RequireHttpsMetadata = false;
      options.ApiName = "blog.core.api";
      options.SupportedTokens = IdentityServer4.AccessTokenValidation.SupportedTokens.Jwt;
      options.ApiSecret = "api_secret";

  })

```
  
### 如何在Swagger中配置？
很简单，直接在 `Swagger` 中直接接入 `oauth、Implicit` 即可：  

```
 //接入identityserver4
 c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
 {
     Type = SecuritySchemeType.OAuth2,
     Flows = new OpenApiOAuthFlows
     {
         Implicit = new OpenApiOAuthFlow
         {
             AuthorizationUrl = new Uri($"http://localhost:5004/connect/authorize"),
             Scopes = new Dictionary<string, string> {
                 {
                     "blog.core.api","ApiResource id" // 资源服务 id
                 }
             }
         }
     }
 });

```
  
然后在 `IdentityServer4`  项目中，做指定的修改：  

```
 new Client {
     ClientId = "blogadminjs",
     ClientName = "Blog.Admin JavaScript Client",
     AllowedGrantTypes = GrantTypes.Implicit,
     AllowAccessTokensViaBrowser = true,

     RedirectUris =
     {
         "http://vueadmin.neters.club/callback",
         // 这里要配置回调地址
         "http://localhost:8081/oauth2-redirect.html" 
     },
     PostLogoutRedirectUris = { "http://vueadmin.neters.club" },
     AllowedCorsOrigins =     { "http://vueadmin.neters.club" },

     AllowedScopes = {
         IdentityServerConstants.StandardScopes.OpenId,
         IdentityServerConstants.StandardScopes.Profile,
         "roles",
         "blog.core.api"
     }
 },

```



## Authorization-JWT 

如果你不想使用 `IdentityServer4` 的话，也可以使用 `JWT` 认证，同样是是`Blog.Core\Blog.Core\Extensions` 文件夹下的 `AuthorizationSetup.cs` 中有关认证的部分：  

```
 1.添加JwtBearer认证服务
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = tokenValidationParameters;
    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // 如果过期，则把<是否过期>添加到，返回头信息中
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
})

```


## AutoMapper

使用 `AutoMapper` 组件来实现 `Dto` 模型的传输转换，具体的用法，可以查看：   
`Blog.Core\Blog.Core\Extensions` 文件夹下的 `AutoMapperSetup.cs` 扩展类，  
通过引用 `AutoMapper` 和 `AutoMapper.Extensions.Microsoft.DependencyInjection` 两个 `nuget` 包，并设置指定的 `profile` 文件，来实现模型转换控制。

```
// 比如如何定义：
 public class CustomProfile : Profile
 {
     /// <summary>
     /// 配置构造函数，用来创建关系映射
     /// </summary>
     public CustomProfile()
     {
         CreateMap<BlogArticle, BlogViewModels>();
         CreateMap<BlogViewModels, BlogArticle>();
     }
 }


// 比如如何使用
models = _mapper.Map<BlogViewModels>(blogArticle);

```
  
具体的查看项目中代码即可。  




## CORS

本项目使用的是 `nginx` 跨域代理，但是同时也是支持 `CORS` 代理的，  
具体的代码可以查看：  
`Blog.Core\Blog.Core\Extensions` 文件夹下的 `CorsSetup.cs` 扩展类，  
通过在 `appsettings.json` 文件中配置指定的前端项目 `ip:端口` ，来实现跨域：  

```

  "Startup": {
    "Cors": {
      "IPs": "http://127.0.0.1:2364,http://localhost:2364,http://localhost:8080,http://localhost:8021,http://localhost:1818"
    },
    "ApiName": "Blog.Core"
  },

```


## DI-AutoFac
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广

## DI-NetCore
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广

## Filter
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广

## Framework 

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## GlobalExceptionsFilter

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## HttpContext

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Log4 

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## MemoryCache

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Middleware

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## MiniProfiler

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广

## publish
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广


## Redis

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Repository
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## SeedData

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## SignalR

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## SqlSugar

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## SqlSugar-Codefirst&DataSeed

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## SqlSugar-SqlAOP

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Swagger

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## T4

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Test-xUnit

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## Temple-Nuget 
精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
## UserInfo 

精力有限，还是更新中...   
如果你愿意帮忙，可以直接在GitHub中，提交pull request，   
我会在后边的贡献者页面里，列出你的名字和项目地址做推广
