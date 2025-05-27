#!/bin/bash

# Inicializace User Secrets (přidá UserSecretsId do souboru .csproj)
dotnet user-secrets init

# Nastavení tajných hodnot (nahraďte <váš-klíč> skutečnou hodnotou)
dotnet user-secrets set "JWT:Key" "vysoce-tajny-vaseklic-32b-need-to-be-very-long--at-least-256-bits-or-something-..."
dotnet user-secrets set "JWT:Issuer" "https://localhost:7055"
dotnet user-secrets set "JWT:Audience" "https://localhost:7055"

# toto je pouze pro moje jednoduche testovani, az to bude live, tak to zmenim
