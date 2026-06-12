# GitHub Setup

The GitHub CLI is not installed on this machine, so this repository was prepared locally.

## Option 1: GitHub Website

1. Create a new public repository named `meetsettle`.
2. Use this description: `Open-source meetup attendance and settlement toolkit`.
3. Do not initialize it with README, license, or `.gitignore`.
4. Run:

```bash
git remote add origin https://github.com/sbpark-escape/meetsettle.git
git branch -M main
git push -u origin main
```

## Option 2: GitHub CLI

After installing and authenticating `gh`, run:

```bash
gh repo create meetsettle --public --description "Open-source meetup attendance and settlement toolkit" --source=. --remote=origin --push
```
