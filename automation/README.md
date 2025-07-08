# Ansible server configuration automation

## Installing ansible
Install python:
```bash
sudo apt update
sudo apt install python3
```

Install pipx for package managing:
```bash
sudo apt install pipx
```

Install ansible:
```bash
pipx install --include-deps ansible
```

Confirm installation:
```bash
ansible --version
```

Use ssh key-based authentication to have ansible's ssh connection be fully automated.
