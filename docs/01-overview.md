# Finance Tracker - Project Overview

## Purpose and intended reader
This article explains what the Finance Tracker service is and what the first release covers. Read it first: it defines the terms used in all other articles.

## Problem it solves
A user wants to track money across several accounts in different currencies (RUB, USD). The service keeps accounts, income/expense operations, transfers between accounts, and per-period statistics, and exposes them through a REST API.

## Scope of the first release
- Registration and JWT authentication (ASP.NET Core Identity).
- Accounts (one currency per account) with derived balances.
- System categories and user categories.
- Operations (income/expense) with filters, sorting and paging.
- Transfers between the user's own accounts of the same currency.
- Statistics for a period (totals, by category, timeline).

Out of scope: multi-currency conversion, budgets, external data sources, file attachments.

## Key terms
- **Account**: a wallet in one currency. Balance is derived, never stored.
- **Category**: groups operations; IsSystem = true means shared and read-only.
- **Operation**: a single income or expense on one account.
- **Transfer**: a movement of money between two accounts of the same user and same currency.
- **Balance (R1)**: StartAmount + Σ income − Σ expense + Σ incoming transfers − Σ outgoing transfers.