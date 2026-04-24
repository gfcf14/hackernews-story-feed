## HackerNews Story Feed: Implementation journey alongside Claude

This README.md will entail the process I took to create this simple app to fetch stories from HackerNews, including prompts and what was modified and/or rejected from the code generation.

**Prompt #1:** Although I had found a few boilerplates online which could allow me to create a .NET Core backend in C#, tied to a frontend in Angular, I assumed from the start that these boilerplates could potentially bring about code or dependencies that, given the simplicity of the assignment, would do more than what I'd need. Though we have amazing computers today, I wanted to err on the side of simplicity and prepare my own, ensuring I use as few dependencies as possible. Though I had developed a C# backend before, and an Angular frontend before, I have never started them together, so I thought of asking Claude the following:

`Could you please prepare me a small boilerplate with a C# .NET Core in the backend and Angular in the frontend, ensuring we use as few dependencies as possible?`

As expected, Claude provided me with most of what I'd need (services, models, calls, in both frontend and backend), even if only barebones but making good assumptions from the project name. However, though I'd feel ok with most of this, I'd first need to run it and understand what it can do thus far. And, as expected, issues arose:

1. Backend wouldn't run because although we needed an application builder to load our relevant services (e.g. CORS for frontend communication), Claude must have misunderstood that we needed to create a builder from the web application builder, not that we needed a build from the web application. So changing this failing line:

```
var builder = WebApplicationBuilder.CreateBuilder(args);
```

to this:

```
var builder = WebApplication.CreateBuilder(args);
```
Fixed the build issue that allowed the backend to run

2. Frontend, though working, initially made use of `HttpClientModule` which is deprecated. This is interesting considering Claude had enough understanding to create a modern version Angular app (18), but not enough to not notice deprecated modules. Since the recommended approach was to use `provideHttpClient`, I modified this to ensure the `main.ts` file includes it, that way we also get the advantage of dependency-injection from anywhere in the app, then removed the old usage of `HttpClientModule` and redundant assignment to the `StoryService` service in the providers.

Fixing these two made the initial app ready for pushing to github and to deploy. So although I also asked Claude to generate a deploy.yml, this would understandably fail because I didn't have `an azure login action` prior to the deployment, as noted in the github build. This meant heading over to portal.azure.com, set up an application with a federated credential to allow deployment from github, creating a web app via App Services to host the deployment, and add the necessary credentials to github as repo secrets. This time, ChatGPT helped me through this process as it has been very helpful for deployment setups in the past. 

**Prompt #2:** Now that the "skeleton" of the app is ready and deployed, since the app is basically already following the requested structure I decided to ask Claude:

`From the structure of the project, we already have components that display a story, but we hade different requirements <pasted  some of the missing requirements here> which indicate that we should: 1) modify the backend to make calls to the HackerNews API instead of test data, update the model accordingly, return values based on incoming filters, and 2) modify the frontend to display stories as per the actual model, find stories by typing a value, and paginate. Please also include test cases so we know http calls behave correctly and errors are safely handled in the backend, and component creation, loading, search and pagination behavior, and sorting for the frontend`

This produced most of the code, albeit with minor issues:
- In the backend, the app would not run. This was because Claude forgot to include specific dependencies (such as Moq.Protected) where needed
- In the frontend, the app would not run because I was now using Math to calculate page numbers, but this wasn't included as a variable in the component class. Adding this variable solved the issue

**Prompt #3:** Lastly, though the app was complete at this point, some "aesthetics" was needed to make this app both easier on the eyes and a bit more user friendly. Despite everything Claude had already done, I noticed we were missing:

- Proper pagination control placement. Current setup would leave it all the way to the bottom
- Debounce logic for the input. Though I could press `Search` or press ENTER, it would look better if it reloads with what's written after some time of inactivity
- Pagination is not stored. This should be stored at least as a url param
- Claude chose an ugly orange color for most of the app (though this one is a personal preference so I'd change it to green)

At this point and since it's all UI, I decided to handle the pagination control placement myself and the color change, while I asked Claude:

`Though the website now looks good, there are a couple of UI fixes we need to make: 1) It would be nice to have debounce logic for the search input, so that even if we can still press enter or the search button, the search would be automatic if left inactive, and 2) we should have url params that indicate the page number.`

With that, the last modifications for the site were complete and ready for review.