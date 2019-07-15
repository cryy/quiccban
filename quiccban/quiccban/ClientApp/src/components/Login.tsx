import * as React from 'react';
import { Typography, Divider, Button, Grid, makeStyles, createStyles, Theme } from "@material-ui/core";

const useStyle = makeStyles((theme: Theme) =>
    createStyles({
        title: {
            fontWeight: 200,
            fontSize: "42px",
            [theme.breakpoints.down('sm')]: {
                fontSize: "32px"
            },
            textAlign: "center"
        },
        divider: {
            marginTop: "3px",
            marginBottom: "4px"
        },
        gridItem: {
            textAlign: "center",
            top: "50%",
            left: "50%",
            position: "absolute",
            transform: 'translate(-50%, -50%)'
        }
    }),
);

export default function Login() {

    const classes = useStyle();

    return (
        <Grid container justify="center" alignItems="center" direction="column" spacing={0}>
            <Grid item xs={3} className={classes.gridItem}>
                <Typography className={classes.title}>quiccban web UI</Typography>
                <Divider className={classes.divider} />
                <Button variant="outlined" color="primary" href="/api/auth/login">
                    Login with Discord
                </Button>
            </Grid>
        </Grid>
    );
}