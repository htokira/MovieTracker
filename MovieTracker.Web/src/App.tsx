import { Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import PrivateRoute from "./components/PrivateRoute/PrivateRoute";
import CatalogPage from "./pages/CatalogPage/CatalogPage";
import CollectionPage from "./pages/CollectionPage/CollectionPage";
import LoginPage from "./pages/AuthPage/LoginPage";
import MovieDetailsPage from "./pages/MovieDetailsPage.tsx/MovieDetailsPage";
import RegisterPage from "./pages/AuthPage/RegisterPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route element={<Layout />}>
        <Route index element={<CatalogPage />} />
        <Route path="movie/:id" element={<MovieDetailsPage />} />
 
        <Route element={<PrivateRoute />}>
          <Route path="collection" element={<CollectionPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
