# CS499-Senior-Project-Group-4
This is the group 4 senior project for fall 2026. We will be initializing automization in order to speed up the working pace within a projects flow in the workspace.

## Email notifications

The API sends email to a task's assigned users when its status changes, and when a task becomes available after dependency resolution. Configure SMTP on the API host; credentials should be stored in .NET user-secrets for local development or supplied through environment variables in deployment, never committed to the repository.

For local development, initialize user-secrets for the API project and set the SMTP values (replace the example values):

```powershell
dotnet user-secrets init --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:Host" "smtp.example.com" --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:Port" "587" --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:UseSsl" "false" --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:UserName" "smtp-user" --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:Password" "smtp-password" --project HorizonLunaris.Api
dotnet user-secrets set "Email:Smtp:FromName" "Horizon Lunaris" --project HorizonLunaris.Api
```

The Automations page lets a Project Manager save the sender email address. If none is saved, the API uses `Email:Smtp:FromEmail`. `UseSsl=false` uses STARTTLS (commonly port 587); `UseSsl=true` uses SSL-on-connect (commonly port 465). If SMTP is not configured, status changes still succeed but email is skipped; the Automations page indicates whether SMTP is ready.
