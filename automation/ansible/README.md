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

## Example Run

Use `-K` flag to prompt for sudo password.
Use `J` to prompt for the vault password.
```bash
ansible-playbook -i inventory.ini site.yaml -K -J
```

# Ansible in WSL

Windows users typically opt to use Ansible through WSL.

A recommended approach is to create an `ansible.cfg` and `.vault_pass` file.

Populate the `.vault_pass` file with your Ansible Vault password (no new line at the end).

Example content of `ansible.cfg`:

```config
[defaults]
vault_password_file = ~/dkef-ansible/.vault_pass
```

Use the environment variable to to use your config file:

```bash
export ANSIBLE_CONFIG=~/dkef-ansible/ansible.cfg
```
