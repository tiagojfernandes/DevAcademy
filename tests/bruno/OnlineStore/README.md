# OnlineStore - Bruno Collection

Open this folder in [Bruno](https://www.usebruno.com/) (File -> Open Collection),
select the **Local** environment, and fill in `adminPassword` and `newPassword`
before running anything. The passwords are intentionally blank in the
committed file so secrets don't end up on GitHub.

The collection is a flat numbered list, meant to be run top-to-bottom.

## Requests

| # | Request | Scenario |
|---|---|---|
| 01 | Login (Admin) | Happy path, saves `adminToken` |
| 02 | Register (New User) | Happy path, saves `userToken` |
| 03 | Login (Wrong Password - expect 401) | Unauthorized |
| 04 | List Products | Anonymous read |
| 05 | Create Product (Admin) | Happy path, saves `productId` |
| 06 | Get Product (Missing - expect 404) | Not Found |
| 07 | Place Order | Happy path (needs items in cart first) |

## Notes

- Requests 01 and 02 must run first so the auth tokens get captured into
  the environment.
- `Place Order` only succeeds if the user already has items in their cart.
  Add an item to the cart with `POST /api/cart/items` using the user token
  before running it.
