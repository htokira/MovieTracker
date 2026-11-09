import { Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import PrivateRoute from "./components/PrivateRoute/PrivateRoute";
import PublicOnlyRoute from "@/components/PublicOnlyRoute/PublicOnlyRoute";
import CatalogPage from "./pages/CatalogPage/CatalogPage";
import CollectionPage from "./pages/CollectionPage/CollectionPage";
import LoginPage from "./pages/AuthPage/LoginPage";
import MovieDetailsPage from "./pages/MovieDetailsPage.tsx/MovieDetailsPage";
import NotFoundPage from "@/pages/NotFoundPage/NotFoundPage";
import ProfilePage from "@/pages/ProfilePage/ProfilePage";
import RegisterPage from "./pages/AuthPage/RegisterPage";

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
 
        <Route element={<PrivateRoute />}>
          <Route path="profile" element={<ProfilePage />} />
          <Route path="collections" element={<CollectionPage />} />
        </Route>
 
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}

export default App;
