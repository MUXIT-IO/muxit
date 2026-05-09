# Security Policy

## Reporting a Vulnerability

If you believe you have found a security vulnerability in Muxit, please
**do not** open a public issue. Instead, report it privately through
GitHub's Private Vulnerability Reporting:

- Go to <https://github.com/muxit-io/muxit/security/advisories/new>, or
- Open the **Security** tab at the top of this repository and click
  **Report a vulnerability**.

This sends your report directly to the maintainers and is not visible to
the public until we publish an advisory.

### What to include

To help us triage quickly, please include as much of the following as
you can:

- A clear description of the vulnerability and its impact.
- Steps to reproduce, including any specific configuration, hardware,
  or environment.
- The version of Muxit affected (run `muxit --version`).
- Any logs, traces, or proof-of-concept code that demonstrates the
  issue.
- Whether you would like to be credited in the advisory.

### What to expect

- We aim to acknowledge new reports within **3 business days**.
- We will investigate and keep you informed of our progress.
- We follow **coordinated disclosure**: a fix is prepared and released
  before a public advisory is published.
- Critical issues are prioritized; lower-severity issues are scheduled
  into the regular release cycle.

## Supported Versions

While Muxit is in beta, only the **latest released version** receives
security fixes. Older versions are not supported. Once Muxit reaches
1.0, this policy will be updated with a versioned support matrix.

## Scope

This policy covers:

- The Muxit application binary distributed via the install scripts in
  this repository.
- The Muxit Driver SDK (`sdk/`) and driver templates (`templates/`).

Out of scope:

- Third-party drivers in the driver registry — please report those to
  their respective publishers.
- The hardware controlled by Muxit — physical safety of instruments
  and equipment is the operator's responsibility.

## Acknowledgements

Reporters who follow this policy and help us improve Muxit's security
will be credited in the release notes for the fix, with their
permission.
