import * as React from 'react';
import { createStyles, Theme, makeStyles, createMuiTheme, MuiThemeProvider } from '@material-ui/core/styles';
import Grid from '@material-ui/core/Grid';
import CircularProgress from '@material-ui/core/CircularProgress';
import { ThemeProvider } from '@material-ui/styles';
import { Typography, CssBaseline } from '@material-ui/core';
import Login from './components/Login';
import Home from './components/Home';
import { Route, Switch } from 'react-router';
import Preloader from './components/Preloader';
import Navbar from './components/Navbar';

const useStyle = makeStyles((theme: Theme) =>
    createStyles({
        progress: {
        },
    }),
);

const theme = createMuiTheme({
    typography: {
        fontFamily: "'Poppins', sans-serif"
    },
    palette: {
        background: {
            default: "#f8f8f8"
        }
    }
});

export interface User {
    id: string;
    avatarHash: string;
    discriminator: string;
    username: string;
    flags: number;
    premiumType?: number;
}


export default function App() {

    const [isConnected, setIsConnected] = React.useState(false);
    const [didError, setDidError] = React.useState(false);
    const [didFetchData, setDidFetchData] = React.useState(false);
    const [user, setUser] = React.useState<User | undefined>(undefined);

    const classes = useStyle();

    React.useEffect(() => {

        fetch("/api/context/user", { method: "GET" })
            .then(x => {
                setTimeout(() => {

                    if (x.status != 200) {
                        setDidFetchData(true);
                    }
                    else (x.json() as Promise<User>).then(y => { setUser(y); setDidFetchData(true); });
                }, 900);
                

            })
        

        return () => {
            
        }
    }, []);




    return (
        <MuiThemeProvider theme={theme} >
            <CssBaseline />
            <Navbar user={user} />
            {didFetchData ? (
                <Switch>
                    <Route exact path="/" render={(props) => <Home {...props} user={user} />} />
                </Switch>
            ) : <Preloader />}
        </MuiThemeProvider>
    );

}

