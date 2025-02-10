```mermaid

graph TD
    Landing[Landing Page] --> Movies[Movies by Category]
    Landing --> Login[Login/Logout]
    Landing --> Search[Search]
    
    Movies --> Featured[Featured Movies]
    Movies --> Recent[Recently Added]
    Movies --> TopRated[Top Rated]
    Movies --> RomCom[Rom-Com]
    Movies --> Horror[Horror/Thriller]
    Movies --> Action[Action]
    Movies --> SciFi[Sci-Fi]
    
    Login --> UserProfile[User Profile]
    UserProfile --> MyReviews[My Reviews]
    MyReviews --> CreateReview[Create Review]
    MyReviews --> EditReview[Edit Review]
    MyReviews --> DeleteReview[Delete Review]
    
    Movies --> MoviePage[Movie Details Page]
    MoviePage --> Reviews[Reviews Section]
    Reviews --> FilterReviews[Filter Reviews]
    Reviews --> SortReviews[Sort Reviews]


```
