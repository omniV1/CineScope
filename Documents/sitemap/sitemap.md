```mermaid


graph TD
    Landing[Landing Page] --> Movies[Movies by Category]
    Landing --> Login[Login/Logout]
    Landing --> Search[Search]
    
    Movies --> Featured[Featured Movies]
    Movies --> Recent[Recently Added]
    Movies --> TopRated[Top Rated]
    
    
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
