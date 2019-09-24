import * as React from 'react';
import { Typography, Grid, makeStyles, Theme, createStyles } from '@material-ui/core';
import { HookProps } from '../../props/HookProps';
import RecentCases from '../cases/RecentCases';

const useStyle = makeStyles((theme: Theme) =>
    createStyles({
        hello: {
            fontSize: "30px",
            marginTop: "12px",
            textAlign: "center",
        },
        recents: {
            marginBottom: "45px"
        }
    }),
);


export default function Home(props: HookProps) {

    const classes = useStyle({});


    return (
        <Grid
            container
            spacing={0}
            direction="column"
            alignItems="center"
            justify="center"
        >

            <Grid item xs={10} md={6} lg={4} style={{ width: '100%' }}>
                <div style={{ width: '100%' }}>
                    <Typography className={classes.hello}>Hello, {props.user.user.username}.</Typography>
                    <div className={classes.recents}>
                        <RecentCases {...props} />
                    </div>
                </div>
            </Grid>
        </Grid>

    );

}
