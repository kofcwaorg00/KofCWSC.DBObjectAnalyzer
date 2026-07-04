# KofCWSC.DBObjectAnalyzer

---

# Database Object Analyzer V2.0

## Functional Guide

### Purpose

The Database Object Analyzer helps answer one question:

> **"Can I safely modify or remove this database object?"**

To answer that, it combines three sources of information:

* SQL Server object definitions
* SQL dependency analysis
* C# source code references

The result is a complete view of how every stored procedure, function, view, and trigger is used.

---

# Summary Worksheet

This worksheet provides a high-level overview of the database.

It answers questions such as:

* How many database objects exist?
* How many are stored procedures?
* How many are functions?
* How many objects are referenced by the application?
* How many are candidates for deletion?

This is typically the first worksheet to review after running the analyzer.

---

# Database Objects Worksheet

This is the master inventory of every database object.

For each object it displays:

* Object name
* Object type
* Number of API references
* Number of outbound SQL dependencies
* Number of inbound SQL dependencies
* Whether it is a deletion candidate

This worksheet is useful for understanding the overall role of an object.

### Example

```
dbo.funSYS_BuildName

API References      3

SQL Outbound        6

SQL Inbound        37
```

This immediately tells you:

* Three C# files call this function.
* The function calls six other SQL objects.
* Thirty-seven database objects depend on it.

Removing this function would have a significant impact.

---

# Dependencies Worksheet

This worksheet answers:

> **"What does this object use?"**

Each row represents one dependency.

Example:

```
dbo.uspRPT_Directory

↓

dbo.funSYS_BuildName

Function Call
```

This means the report procedure calls the BuildName function.

Use this worksheet when:

* Modifying a stored procedure
* Refactoring SQL
* Understanding business logic
* Identifying downstream dependencies

---

# Referenced By Worksheet

This is the reverse of the Dependencies worksheet.

It answers:

> **"Who uses this object?"**

Example:

```
dbo.funSYS_BuildName

Referenced By

dbo.uspRPT_Directory

dbo.uspRPT_GetRollCall

dbo.uspWEB_GetSOSView
```

This worksheet is extremely valuable before changing an object.

It identifies every object that depends on it.

---

# Candidates Worksheet

This worksheet lists potential deletion candidates.

A candidate has:

* No API references
* No SQL objects calling it
* No outbound dependencies

These objects should be reviewed manually before deletion.

**Important**

A candidate is **not automatically safe to delete**.

Possible reasons an object appears unused include:

* Dynamic SQL
* SQL Agent jobs
* External applications
* Manual execution by administrators
* Scheduled maintenance scripts

Always verify before removing production objects.

---

# Source References Worksheet

This worksheet links SQL objects back to the C# application.

Each row shows:

* Database object
* Source file
* Line number
* Type of reference
* Source code

This answers:

> **"Where is this procedure called in the application?"**

This is invaluable when modifying APIs or debugging application behavior.

---

# Typical Workflows

## Before deleting a stored procedure

1. Locate the object in **Database Objects**.
2. Confirm it is listed in **Candidates**.
3. Search **Referenced By** to verify nothing depends on it.
4. Search **Source References** to ensure the application never calls it.
5. Check for SQL Agent jobs or dynamic SQL before deleting.

---

## Before modifying a function

1. Find the function in **Referenced By**.
2. Review every dependent object.
3. Estimate the impact of the change.
4. Test all dependent procedures.

---

## Understanding a report

Locate the report procedure in **Dependencies**.

Example:

```
dbo.uspRPT_Directory
```

The worksheet immediately shows every function and procedure used by the report.

This provides an excellent overview of the report's implementation.

---

## Finding heavily reused objects

Sort the **Database Objects** worksheet by:

```
SQL Inbound
```

Objects with the highest inbound count are the most reused components in the database.

Changes to these objects should receive additional testing.

---

## Finding complex procedures

Sort the **Database Objects** worksheet by:

```
SQL Outbound
```

Procedures with many outbound dependencies are typically the most complex and may benefit from refactoring.

---

# Recommended Usage

I recommend using the workbook in this order:

1. **Summary** – Understand the overall database.
2. **Database Objects** – Locate the object of interest.
3. **Referenced By** – Determine who depends on it.
4. **Dependencies** – Determine what it depends on.
5. **Source References** – Locate the application code.
6. **Candidates** – Review possible cleanup opportunities.

This sequence gives you both the "upstream" and "downstream" impact of any change.

---

# Current Scope (V2.0)

The analyzer currently recognizes:

* Stored procedure calls (`EXEC`, `EXECUTE`)
* Scalar function calls
* Cross-object SQL dependencies
* API references from the C# application

It intentionally does **not** attempt to resolve:

* Dynamic SQL (`EXEC(@sql)`)
* `sp_executesql`
* SQL Agent jobs
* External applications
* Linked servers

These cases require manual review.

---

## My final recommendation

If I were releasing this tool to your development team, I'd include this guide as **README.md** in the repository and also add it as a **Documentation** worksheet in the workbook itself. That way, anyone who receives the workbook can immediately understand what each worksheet means and how to use it, without having to ask the original developer. I think that would be a fitting finishing touch to V2.0.
