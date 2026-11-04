import { Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import PrivateRoute from "./components/PrivateRoute";
import CatalogPage from "./pages/CatalogPage";
import CollectionPage from "./pages/CollectionPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route element={<Layout />}>
        <Route index element={<CatalogPage />} />
 
        <Route element={<PrivateRoute />}>
          <Route path="collection" element={<CollectionPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
