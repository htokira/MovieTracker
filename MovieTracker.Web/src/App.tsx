import { Route, Routes } from "react-router-dom";
import PrivateRoute from "@/features/auth/PrivateRoute";
import PublicOnlyRoute from "@/features/auth/PublicOnlyRoute";
import Layout from "@/layouts/Layout/Layout";
import AllReviewsPage from "@/pages/AllReviewsPage/AllReviewsPage";
import LoginPage from "@/pages/AuthPage/LoginPage";
import RegisterPage from "@/pages/AuthPage/RegisterPage";
import CatalogPage from "@/pages/CatalogPage/CatalogPage";
import MyMoviesPage from "@/pages/MyMoviesPage/MyMoviesPage";
import MovieDetailsPage from "@/pages/MovieDetailsPage/MovieDetailsPage";
import NotFoundPage from "@/pages/NotFoundPage/NotFoundPage";
import ProfilePage from "@/pages/ProfilePage/ProfilePage";

function App() {
  return (
    <Routes>
      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>
 
      <Route element={<Layout />}>
        <Route index element={<CatalogPage />} />
        <Route path="movie/:id" element={<MovieDetailsPage />} />
        <Route path="movie/:id/reviews" element={<AllReviewsPage />} />
 
        <Route element={<PrivateRoute />}>
          <Route path="profile" element={<ProfilePage />} />
          <Route path="my-movies" element={<MyMoviesPage />} />
        </Route>
 
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}

export default App;
