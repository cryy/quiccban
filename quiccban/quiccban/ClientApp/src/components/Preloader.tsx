import * as React from 'react';
import { Typography, Divider, Button, Grid, makeStyles, createStyles, Theme, CircularProgress } from "@material-ui/core";

const useStyle = makeStyles(() =>
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

export default function Preloader() {

    const classes = useStyle({});


    return <CircularProgress className={classes.loader} size={180} thickness={1.2} />
}