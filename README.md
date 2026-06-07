# To-Do App

A task management app built with Angular and .NET.

---

## What you need installed

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)

---

## How to run it

You need two terminals — one for the backend, one for the frontend.

### 1. Start the backend

```bash
cd ToDoApp.API
dotnet run
```

It will start on `https://localhost:7066`

The API uses an in-memory database, so no setup needed. Data resets every time you restart it.

### 2. Start the frontend

In a second terminal:

```bash
cd my-todo-app.Angular
npm install --legacy-peer-deps
npx ng serve
```

Then open [http://localhost:4200](http://localhost:4200) in your browser.

---

## First time using it

1. Go to [http://localhost:4200](http://localhost:4200)
2. Click **Register** and create an account
3. Log in and you're in

---

## Built with PrimeNG

The UI uses [PrimeNG 21](https://primeng.org) with the Aura theme. 
Components used across the app: `p-dialog`, `p-toast`, `p-button`, `p-select`, `p-datepicker`, `p-checkbox`, `p-tag`, `p-confirmDialog`, `p-progressSpinner`, `p-avatar`, `p-toolbar`, and `p-progressBar`.

---

## Notes

- Backend needs to be running before the frontend, or API calls will fail

## References

The following resources were used as reference material during development. These resources were used for guidance and learning purposes only. The application design, implementation, tests, and documentation are my own work.

### Frontend

- PrimeNG Sakai Template
  - https://github.com/primefaces/sakai-ng/tree/master

### Backend Testing

- Unit Testing a C# .NET Core Web API CRUD Application
  - https://zubairmoosa.medium.com/unit-testing-a-net-core-web-api-crud-640d3a3cff37

- How We Mock JWT in .NET Integration Tests
  - https://blog.devgenius.io/how-we-mock-jwt-in-integration-tests-a253584997ff

- Unit Testing Factory Classes in C#
  - https://stackoverflow.com/questions/42772447/unit-test-factory-class-in-c-sharp

### Official Documentation

- [PrimeNG Documentation](https://primeng.org/)
