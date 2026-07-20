# GraphQL API conventions

This project exposes separate public and admin GraphQL schemas:

- `/graphql` contains public, read-only portfolio data.
- `/graphql/admin` contains authenticated project-management queries and mutations outside development.

The conventions below define the target shape for new fields. Existing fields
can migrate to them one operation at a time.

## Response errors

GraphQL already defines a response envelope with `data` and optional top-level
`errors`. Schema payloads must not add another field named `errors`.

Top-level GraphQL errors represent failures that prevent normal operation,
including:

- Invalid GraphQL syntax, variables, or input coercion
- Authentication or authorization failures during GraphQL execution
- Database, object-storage, and other infrastructure failures
- Unexpected exceptions and programming defects

Unexpected errors must be logged internally and returned with a stable error
code and a safe message. Internal exception details must not be exposed.

Expected mutation failures are returned as `userErrors`. Examples include:

- Domain validation failures
- A missing mutation target
- A duplicate or conflicting value
- An ID that does not belong to the target resource
- An invalid state transition

## Mutation payloads

Every mutation returns a non-null, operation-specific payload. The payload has:

- A nullable, specifically named result such as `project`, `tags`, or
  `deletedProjectId`
- A non-null `userErrors` list, which is empty on success

For example:

```graphql
type UpdateProjectPayload {
  project: Project
  userErrors: [UserError!]!
}

type UserError {
  code: UserErrorCode!
  message: String!
  field: [String!]
}

enum UserErrorCode {
  VALIDATION
  NOT_FOUND
  CONFLICT
  INVALID_REFERENCE
  INVALID_STATE
}
```

For a normal, non-batch mutation:

- Success returns result data and an empty `userErrors` list.
- Expected failure returns null result data and one or more `userErrors`.
- Unexpected failure produces a top-level GraphQL error.

`field` identifies the relevant input path when one exists. For example,
`["input", "title"]` points to the mutation's title input. Errors that apply to
the operation as a whole leave `field` null.

Mutation payloads use domain-specific result names rather than a generic nested
`data` field because the GraphQL response already has a top-level `data` field.

## Queries

Queries return their data directly and do not use mutation payloads or
`userErrors`.

- A missing single-resource lookup returns null.
- A collection with no matching records returns an empty collection.
- Potentially unbounded collections use a consistent paging model.
- Execution and infrastructure failures use top-level GraphQL errors.

## Naming

- Query fields use resource names such as `projects`, `tags`, and `projectById`.
- Mutation fields use a verb and resource, such as `createProject`,
  `updateProject`, and `deleteProject`.
- Input types use the mutation name followed by `Input`.
- Payload types use the mutation name followed by `Payload`.
- Error codes are stable enum values intended for client logic. Messages are
  human-readable and can change without becoming a client contract.

## Transactions and partial success

A mutation that changes related database records should use one transaction and
avoid partial success unless partial results are an intentional part of its
schema.

Batch operations must report item-level outcomes explicitly rather than using a
single ambiguous success value.

Direct-to-storage image uploads are a multi-step exception: preparation,
browser upload, and finalization cannot share one database transaction. Their
API operations must therefore be idempotent so interrupted steps can be safely
retried.

## Client handling order

Clients handle mutation responses in this order:

1. Handle transport and top-level GraphQL errors.
2. Handle payload `userErrors`.
3. Read the successful result data.

The client must not interpret null result data by itself as a complete error
contract.
