This repository owns its implementation rules. Keep local AGENTS.md, skills, ADRs and docs current.

## Task Status via Pull Requests

Every implementation PR must reference its task issue with a closing keyword in the
PR description, e.g. `Closes #123`. The `PR Task Status` workflow uses this to move
the task's `status:*` label: PR opened -> `status:in-review`, PR merged ->
`status:done`. Set the task to `status:in-progress` when you start work.

See the platform label model: `docs/process/label-model.md` in the platform repository.
