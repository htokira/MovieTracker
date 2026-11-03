import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';

const CatalogPlaceholder = () => <div style={{ padding: '2rem' }}>тут буде каталог колись</div>;
const CollectionPlaceholder = () => <div style={{ padding: '2rem' }}>на днях буде колекція</div>;

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<CatalogPlaceholder />} />
          <Route path="collection" element={<CollectionPlaceholder />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
