******************************
Assignment 1:			
******************************
Website:
This web site for Actors and movies rating, somthing like IMDB, You can create an actor or movie.

Database:
The movie database consists of two tables, The movies table, which has 4 fields (title, rating, relase date and genre)
and foreign Key actor id which refer to actor table. Actor table has 2 field (Name and Age), both tables has primary id keys.
The movie database is one to many. The Actor can has alot of movies.
(Note: it should be many to many but i made one to many for the prompt of the assignmet)

Models:
Model created from database Movies, using Entity Framework by Scaffold-DbContext

Controllers:
There are 3 controllers: Home, Movies and Actors

Views:
Every Controlers auto generate its CRUD views.

Layout:
Footer adjusted and Navbar

Css
I added Some Css theme from php course in semster 2.

JavaScript
I added sortable javascript file for sorting the tables.

******************************
Assignment 2:				
******************************

Authentication:
Authentication models and tables added. Log in and registration are now functional

Authorization:
Authorization added, there is two level of authorization customer, can see movies or actors tables and can access details and admin,
which can create,edit delete movies or actors. Anonymous can see tables only without seeing the details.
Create, Edit and Delete contolers make used only by admin role only any other role will give access denied


******************************
Assignment 3:				
******************************
Impotant Note: Because my site was a rating site like IMDB so i change it to Movies store so i can sell something 
for sake of assignmnet 3A and 3B  

Functional “Shop and Cart” are visible to users that are logged in or anonymous.
Shopping Cart populates from Categories, and each item in your categories is populated by products from DBase.
Cart button take users to cart
User’s Cart adds a running total of items in cart (Quantity x Price)
The Cart navigated to a Checkout View.
Checkout View will allow the user to input FN, LN, Address, City, Province, Postal Code and Phone number.
STRIPE payment gateway added
