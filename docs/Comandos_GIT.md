
# Git Flow, Git Essentials e C# Básico

## Git Flow

### 1. Inicialização do Repositório
```sh
git init
```

### 2. Configuração do Git Flow
```sh
git flow init
```

### 3. Criando uma Nova Feature
```sh
git flow feature start minha-feature
```

### 4. Finalizando uma Feature
```sh
git flow feature finish minha-feature
```

### 5. Criando um Release
```sh
git flow release start v1.0.0
```

### 6. Finalizando um Release
```sh
git flow release finish v1.0.0
```

### 7. Correção de Bugs (Hotfix)
```sh
git flow hotfix start correcao-importante
```

### 8. Finalizando um Hotfix
```sh
git flow hotfix finish correcao-importante
```

---

## Comandos Essenciais do Git

### Configurações Iniciais
```sh
git config --global user.name "Seu Nome"
git config --global user.email "seuemail@exemplo.com"
```

### Clonar um Repositório
```sh
git clone https://github.com/usuario/repositorio.git
```

### Verificar Status
```sh
git status
```

### Adicionar Arquivos
```sh
git add .
git add arquivo.ext
```

### Remover Arquivos
```sh
git rm arquivo.ext
```

### Commitar Alterações
```sh
git commit -m "Mensagem do commit"
```

### Enviar para o Repositório Remoto
```sh
git push origin nome-da-branch
```

### Puxar Alterações do Remoto
```sh
git pull origin nome-da-branch
```

### Listar Branches
```sh
git branch
git branch -r
git branch -a
```

### Criar Nova Branch
```sh
git checkout -b nome-da-branch
```

### Trocar de Branch
```sh
git checkout nome-da-branch
```

### Mesclar Branch
```sh
git checkout main
git merge nome-da-branch
```

### Remover Branch Local
```sh
git branch -d nome-da-branch
git branch -D nome-da-branch
```

### Remover Branch Remota
```sh
git push origin --delete nome-da-branch
# ou
git push origin :nome-da-branch
```

### Ver Log de Commits
```sh
git log --oneline --graph --all
```

### Resetar Arquivos
```sh
git checkout -- arquivo.ext
```

### Resetar Último Commit (Manter Arquivos)
```sh
git reset --soft HEAD~1
```

### Resetar Último Commit (Desfaz Tudo)
```sh
git reset --hard HEAD~1
```

---

## Comandos Essenciais de C# (.NET)

### Compilação e Execução
```sh
dotnet build
dotnet run
dotnet watch run
```

### Criar um Novo Projeto
```sh
dotnet new console -n NomeDoProjeto
```

### Gerenciamento de Pacotes
```sh
dotnet add package NomeDoPacote
dotnet restore
```

### Pacotes do Entity Framework Core
```sh
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## UrlHelper.Action (ASP.NET MVC)

### Documentação: https://learn.microsoft.com/pt-br/dotnet/api/system.web.mvc.urlhelper.action?view=aspnet-mvc-5.2

### Descrição:
O método `Action` gera uma URL para uma ação específica em um controlador.

### Assinatura:
```csharp
public virtual string Action (string actionName, string controllerName);
```

###  Parâmetros:
- actionName – Nome da ação
- controllerName – Nome do controlador

###  Retorno:
- String contendo a URL gerada.

---

## By Arthur Gaspare
