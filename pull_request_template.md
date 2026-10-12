## KITOS Pull request template

### Description

_Describe what was changed in this branch, and WHY it was changed._

### Checklist

The following procedure dictates the steps needed before a Pull request can be merged into master.

- [ ] **Implement**:
      _All requirements are implemented and unit tests are green_
- [ ] **Merge master into branch / rebase with master**:
      _Make sure you are testing your changes and how they co-exist with the latest version of master_
- [ ] **Green on integration**:
      _All integration tests are green on integration_

- [ ] **Add a description**
      _Under "Description" above, explain what was changed in this branch, and WHY it was changed_

- [ ] **Database compatibility**:
      _When database changes are included, both SQL Server and PostgreSQL versions have been updated, reviewed, and tested._

- [ ] **Warnings cleanup**:
      _Files with changes should be checked for warnings. Any warnings found should be fixed_

- [ ] **Quality gate green**:
      _`pwsh ./scripts/quality-check.ps1` passes locally and the "PR quality gate" check is green. Any WARN items are addressed or explained above (see docs/CODE_QUALITY.md)_
