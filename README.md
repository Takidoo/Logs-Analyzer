# 📋 Log Analyzer

> **Analyseur de logs** moderne pour Windows — interface sombre *flat design*, filtres par niveau, recherche instantanée et export CSV.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-0078D4?style=flat-square&logo=windows)](https://learn.microsoft.com/dotnet/desktop/wpf/)
[![Platform](https://img.shields.io/badge/platform-Windows-00A4EF?style=flat-square&logo=windows11)](https://www.microsoft.com/windows)

---

## ✨ Aperçu

<p align="center">
  <img src="LogAnalyser/thumbnail/exemple.png" alt="Capture d’écran — Log Analyzer en mode sombre avec sample.log" width="920">
  <br>
  <sub><i>Exemple avec <code>sample.log</code> — filtres ERROR / WARN / INFO / DEBUG, grille et zone « Raw line ».</i></sub>
</p>

---

## 🎯 Fonctionnalités

| | |
|:---|:---|
| 📂 **Ouverture** | Fichiers `.log`, `.txt` ou tout fichier texte |
| 🔍 **Recherche** | Filtre en direct sur message, source, horodatage et niveau |
| 🎚️ **Filtres** | Pills **ERROR** · **WARN** · **INFO** · **DEBUG** (+ **TRACE** regroupé avec DEBUG) |
| 📊 **Statistiques** | Compteurs par niveau et total d’entrées |
| 🎨 **Lisibilité** | Badges colorés, lignes teintées, police monospace pour les données |
| 📤 **Export** | **Export CSV** des lignes **actuellement filtrées** |
| 🪟 **Fenêtre** | Sans chrome Windows classique, **déplaçable** depuis la barre du haut (double-clic pour maximiser) |

---

## 🧩 Formats de log reconnus

L’application tente de découper automatiquement **horodatage**, **niveau**, **source** et **message**. Exemples supportés :

```text
2026-04-01 08:00:00.001 [INFO] [Startup] Message…
2026-04-01T08:00:01,250 [WARN] Message…
2026-04-01 08:00:02.500 INFO  Database: Message…
[2026-04-01 08:00:05] INFO - Message…
08:00:10.001 INFO Message…
ERROR: Message sans horodatage
```

Les lignes qui ne correspondent à aucun motif restent affichées en entier dans la colonne **Message** (mode brut).

---

## 🚀 Lancer le projet

### Prérequis

- [SDK .NET 9](https://dotnet.microsoft.com/download) (cible **Windows** / **WPF**)

### Depuis le terminal

```powershell
cd LogAnalyser
dotnet run
```

### Exécutable (après build)

```powershell
dotnet build
```

Puis lancer :

`LogAnalyser\bin\Debug\net9.0-windows\LogAnalyser.exe`

---

## 🧪 Fichier d’essai

Un fichier **`sample.log`** est disponible à la racine du dossier **Log** pour tester filtres, recherche et export.

---

## 🛠️ Structure du dépôt

```text
Log/
├── README.md                 ← vous êtes ici
├── sample.log                ← données de démo (optionnel)
└── LogAnalyser/
    ├── LogAnalyser.csproj
    ├── App.xaml
    ├── MainWindow.xaml       ← UI (thème sombre, WindowChrome)
    ├── MainWindow.xaml.cs    ← parsing, filtres, export
    └── thumbnail/
        └── exemple.png       ← capture pour la doc
```

---

## 📝 Licence & contribution

Projet personnel / bac à sable — adaptez le code comme vous voulez.  
Les idées et retours sont les bienvenus 🙌

---

<p align="center">
  <b>Fait avec</b> ☕ <b>et</b> WPF
  <br>
  <sub>Happy log hunting! 🪵✨</sub>
</p>
