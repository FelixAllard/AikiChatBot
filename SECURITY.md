# Security Policy
## Reporting a Security Issue
If you discover a security vulnerability in this repository, please report it immediately by following these steps:

Do not share the vulnerability publicly — report it privately to the repository owner or maintainer.
Contact the repository owner directly via email to [Felix Allard](allardf@aikitech.ca) or through a secure channel.
Provide as much detail as possible about the issue, including steps to reproduce it.
Steps to Take in Case of a Security Leak (e.g., Leaked API Key or Secret)
If an API key, secret, or other sensitive information is found in the repository, follow these steps IMMEDIATELY:

### ✅ 1. Temporarily Make the Repository Private
Go to your GitHub repository page.
Navigate to Settings → General.
Scroll down to the "Danger Zone" section.
Click "Make private" → Confirm the action.

__**👉 Making the repository private will prevent unauthorized access while you fix the issue.**__

---

### ✅ 2. Disable Repository Changes
To prevent further modifications until the leak is addressed, enable branch protection:

Go to Settings → Branches.
Under Branch protection rules, select the main branch (e.g., main).
Enable the following settings:
- ✅ Require a pull request before merging
- ✅ Require status checks to pass before merging
- ✅ Include administrators (optional)
- ✅ Require conversation resolution before merging

---

### 3. Remove the Leaked Key or Secret from Version Control
To completely remove sensitive data from the repository history:

Open a terminal or Git Bash.
Use git filter-repo (or BFG Repo-Cleaner) to remove the key:
```bash
# Install git-filter-repo if not installed
git clone https://github.com/newren/git-filter-repo.git
cd git-filter-repo
make prefix=/usr/local install
```
Remove the secret using git filter-repo:
```bash
git filter-repo --path-glob '*.env' --replace-text <(echo 'SECRET_KEY==>***REMOVED***')
```

If the key was in a specific file, use:


```bash

git filter-repo --path path/to/secret-file --invert-paths
```
Force push the cleaned history to GitHub:

```bash
git push origin main --force
```

---

### ✅ 4. Rotate the Compromised Key or Secret
Go to the service where the key or secret was generated (e.g., AWS, Azure, Google Cloud).
Revoke the old key immediately.
Generate a new key and update your project configuration files (e.g., .env).

---

### ✅ 5. Notify Stakeholders
Inform the team about the security breach.
Document the incident and the steps taken to resolve it.
Perform a post-mortem to understand how it happened and how to prevent similar issues in the future.
Preventative Measures

To avoid future security leaks:

✅ Add .env and sensitive files to .gitignore:

```text
# .gitignore
.env
secrets.json
```
✅ Use a secret-scanning tool (like GitHub Advanced Security or TruffleHog) to automatically detect secrets in the codebase.

✅ Use environment variables for sensitive data instead of hardcoding values.

---

#### Need Help?

If you need further assistance, please contact [Felix Allard](allardf@aikitech.ca).