# Codebase Rules

This document defines the standards and conventions required to maintain a consistent, readable, and scalable codebase.

---

## 1. Core Principles

- Code must prioritize **readability and performance over cleverness**.
- Prefer **explicit and simple solutions** over implicit or complex ones.
- Consistency across the codebase is more important than individual preference.
- Avoid premature optimization; optimize only when necessary and measured.
- Temporary solutions must be clearly marked with `TODO` or `FIXME`.

---

## 2. Architecture Guidelines

- Follow **Single Responsibility Principle**: each class/system should have one clear purpose.
- Separate **game logic / business logic** from **engine/framework-specific code**.
- Use **interfaces and abstractions** to decouple systems.
- Do not introduce new patterns unless there is a clear and justified need.
- Extend existing systems instead of bypassing them.

---

## 3. Naming Conventions

### General
- Names must be **descriptive and intention-revealing**.
- Avoid abbreviations unless they are universally understood.

### Types
- Classes, structs, interfaces: `PascalCase`
- Interfaces must start with `I` (e.g. `IMovementSystem`)

### Variables
- Private fields: `m_camelCase`
- Local variables: `camelCase`
- Constants: `k_camelCase`

### Methods
- Use verbs or verb phrases (`CalculateVelocity`, `TryGetTarget`)
- Boolean-returning methods should start with `Is`, `Has`, `Can`

---

## 4. Code Structure

- Keep methods **short and focused**.
- Avoid deep nesting (max 2–3 levels).
- Early return is preferred over nested conditionals.
- Group related logic together; avoid scattered responsibilities.
- Avoid “magic numbers”; use named constants.

---

## 5. System Design Rules

- Systems must not directly depend on each other unless explicitly designed.
- Communication between systems should happen via:
    - Events
    - Interfaces
    - Message passing

- Avoid tight coupling between systems.
- Systems should be testable in isolation.

---

## 6. Unity-Specific Rules (if applicable)

- Avoid putting logic directly inside `MonoBehaviour` unless necessary.
- Treat `MonoBehaviour` as a **composition layer**, not a logic container.
- Do not abuse `Update`; use centralized update loops when possible.
- Prefer deterministic logic when required (e.g. simulations).
- Avoid `Find`, `GetComponent` in runtime loops.

---

## 7. Performance Guidelines

- Avoid allocations in hot paths.
- Cache frequently used references.
- Use profiling before optimizing.
- Do not sacrifice readability for micro-optimizations without proof.

---

## 8. Error Handling & Logging

- Fail loudly in development, fail safely in production.
- Use meaningful error messages.
- Avoid silent failures.
- Logging must provide actionable information.

---

## 9. Comments & Documentation

- Code should be self-explanatory; comments are for **why**, not **what**.
- Public APIs must be documented.
- Avoid redundant comments.
- Complex logic must include explanation.

---

## 10. Git & Collaboration Rules

- Keep commits **small and focused**.
- Write meaningful commit messages:
    - `Fix: incorrect velocity calculation`
    - `Refactor: decouple movement system`

- Do not commit:
    - Debug code
    - Unused code
    - Temporary hacks

---

## 11. Code Review Expectations

- Code must be understandable without verbal explanation.
- Reviewers should focus on:
    - Architecture consistency
    - Readability
    - Maintainability
- Personal preferences should not override established rules.

---

## 12. Forbidden Practices

- Copy-paste programming without refactoring
- Hidden side effects
- God classes / overly large systems
- Hardcoded dependencies
- Ignoring existing architecture
- Trying to compile unity .cs files

---

## 13. TODO Policy

- Every `TODO` must:
    - Explain the reason
    - Be actionable
- Example:
  ```csharp
  // TODO: Replace with deterministic solver once physics refactor is complete