import * as React from 'react';
import { createStyles, Theme, makeStyles, createMuiTheme, MuiThemeProvider } from '@material-ui/core/styles';
import { CssBaseline } from '@material-ui/core';
import Login from './components/Login';
import Home from './components/routes/Home';
import { Route, Switch } from 'react-router';
import Preloader from './components/Preloader';
import Navbar from './components/Navbar';
import { ISelfUser } from './entities/user/ISelfUser';
import { SnackbarProvider } from 'notistack';
import { APIClient } from './apiclient/APIClient';

const useStyle = makeStyles((theme: Theme) =>
    createStyles({
        loader: {
            position: "absolute",
            marginLeft: "50%",
            left: "-80px",
            top: "50%",
            marginTop: "-80px"
        }
    }),
);

const theme = createMuiTheme({
    typography: {
        fontFamily: "'Poppins', sans-serif"
    },
    palette: {
        background: {
            default: "#f8f8f8",
        }
    }
});



function Render() {

    const [didFetchData, setDidFetchData] = React.useState(false);
    const [user, setUser] = React.useState<ISelfUser | undefined>(undefined);
    const [apiClient, setAPIClient] = React.useState<APIClient>(null);


    const classes = useStyle(theme);

    React.useEffect(() => {

        let apiClient = new APIClient();

        setAPIClient(apiClient);


        apiClient.getSelfInfo().then(user => {
            setTimeout(() => { setUser(user); setDidFetchData(true); }, 900);
        })
            .catch(() => { setTimeout(() => { setDidFetchData(true) }, 900); });

        return () => {

        }
    }, []);

    return (
        <div>
            <Navbar user={user} />
            {didFetchData ?
                (
                    user ?
                        (
                            <Switch>
                                <Route exact path="/" render={(props) => <Home {...props} user={user} apiClient={apiClient} />} />
                            </Switch>
                        ) : <Login />
                ) : <Preloader iclass={classes.loader} />
            }
        </div>
    );

}

export default function App() {
    return (
        <MuiThemeProvider theme={theme} >
            <CssBaseline />
            <SnackbarProvider maxSnack={3}>
                <Render />
            </SnackbarProvider>
        </MuiThemeProvider>
    );
}

