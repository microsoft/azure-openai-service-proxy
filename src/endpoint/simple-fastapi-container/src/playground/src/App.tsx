import "./App.css";
import { Unauthorised } from "./components/Unauthorised";
import { Error } from "./components/Error";
import { Outlet } from "react-router-dom";

function App() {
  return (
    <>
      <section className="App">
        <Outlet />
      </section>
      <Unauthorised />
      <Error />
    </>
  );
}

export default App;
